using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Extensions;
using Clinic.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Clinic.Infrastructure.Errors;
using Microsoft.AspNetCore.Http; // For StatusCodes

namespace Clinic.Infrastructure.Services;

public class VerificationService(
    ICacheService cacheService,
    UserManager<ApplicationUser> userManager
    // IEmailService emailService (future)
    ) : IVerificationService
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private const string OtpPrefix = "otp:email:";
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(10);

    public async Task<Result> SendEmailOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.EmailConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Email already verified"));

        // Generate 6-digit OTP
        var code = new Random().Next(100000, 999999).ToString();
        var cacheKey = $"{OtpPrefix}{email}";

        // Store in Redis with TTL
        await _cacheService.SetAsync(cacheKey, code, OtpTtl, cancellationToken);

        // TODO: Send email
        // For development, we log it or it will be visible in Redis
        // Console.WriteLine($"OTP for {email}: {code}");

        return Result.Succeed();
    }

    public async Task<Result> VerifyEmailOtpAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.EmailConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Email already verified"));

        var cacheKey = $"{OtpPrefix}{email}";
        var storedCode = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

        if (storedCode == null || storedCode != code)
        {
            return Result.Failure(new Error("Auth.InvalidOtp", "Invalid or expired OTP", StatusCodes.Status400BadRequest));
        }

        // Verify user
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        // Clear OTP
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return Result.Succeed();
    }
}
