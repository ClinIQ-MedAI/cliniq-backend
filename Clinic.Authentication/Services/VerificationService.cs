using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Services;
using Clinic.Infrastructure.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Clinic.Authentication.Services;

public class VerificationService(
    ICacheService cacheService,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment) : IVerificationService
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IWebHostEnvironment _environment = environment;

    private const string EmailOtpPrefix = "otp:email:";
    private const string PhoneOtpPrefix = "otp:phone:";
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(10);

    public async Task<Result> SendEmailOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.EmailConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Email already verified"));

        // Generate 5-digit OTP
        var code = GenerateOtp();
        var cacheKey = $"{EmailOtpPrefix}{email}";

        // Store in Redis with TTL
        await _cacheService.SetAsync(cacheKey, code, OtpTtl, cancellationToken);

        // TODO: Send email
        // For development, we log it or it will be visible in Redis
        // Console.WriteLine($"Email OTP for {email}: {code}");

        return Result.Succeed();
    }

    public async Task<Result> SendPhoneOtpAsync(string phone, CancellationToken cancellationToken = default)
    {
        // Phone lookup might vary depending on how it's stored (normalized?)
        // Assuming unique phone numbers
        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phone);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.PhoneNumberConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Phone already verified"));

        // Generate 5-digit OTP
        var code = GenerateOtp();
        var cacheKey = $"{PhoneOtpPrefix}{phone}";

        await _cacheService.SetAsync(cacheKey, code, OtpTtl, cancellationToken);

        // TODO: Send SMS

        return Result.Succeed();
    }

    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.EmailConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Email already verified"));

        var cacheKey = $"{EmailOtpPrefix}{request.Email}";
        var storedCode = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

        if (storedCode == null || storedCode != request.Code)
        {
            return Result.Failure(new Error("Auth.InvalidOtp", "Invalid or expired OTP", StatusCodes.Status400BadRequest));
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return Result.Succeed();
    }

    public async Task<Result> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken = default)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == request.Phone);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.PhoneNumberConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Phone already verified"));

        var cacheKey = $"{PhoneOtpPrefix}{request.Phone}";
        var storedCode = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

        if (storedCode == null || storedCode != request.Code)
        {
            return Result.Failure(new Error("Auth.InvalidOtp", "Invalid or expired OTP", StatusCodes.Status400BadRequest));
        }

        user.PhoneNumberConfirmed = true;
        await _userManager.UpdateAsync(user);
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return Result.Succeed();
    }

    private string GenerateOtp()
    {
        if (_environment.IsDevelopment())
        {
            return "12345";
        }

        return new Random().Next(10000, 99999).ToString();
    }
}
