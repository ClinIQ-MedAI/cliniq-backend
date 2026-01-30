using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Errors;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authentication.Services;

/// <summary>
/// Verification service for email and phone using OTP codes.
/// </summary>
public class VerificationService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context) : IVerificationService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Find valid OTP for this user
        var otp = await _context.Set<OtpCode>()
            .Where(o => o.UserId == user.Id && o.Code == request.Code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null)
            return Result.Failure(new Error("Verification.InvalidCode", "Invalid or expired verification code", StatusCodes.Status400BadRequest));

        // Mark OTP as used
        otp.IsUsed = true;

        // Update user verification status
        user.EmailVerified = true;
        user.EmailConfirmed = true;

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Succeed();
    }

    public async Task<Result> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Find valid OTP for this user
        var otp = await _context.Set<OtpCode>()
            .Where(o => o.UserId == user.Id && o.Code == request.Code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null)
            return Result.Failure(new Error("Verification.InvalidCode", "Invalid or expired verification code", StatusCodes.Status400BadRequest));

        // Mark OTP as used
        otp.IsUsed = true;

        // Update user verification status
        user.PhoneVerified = true;
        user.PhoneNumberConfirmed = true;

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Succeed();
    }

    public async Task<Result> SendEmailOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Generate 6-digit OTP
        var code = GenerateOtpCode();
        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.Set<OtpCode>().Add(otp);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send email with OTP code via email service

        return Result.Succeed();
    }

    public async Task<Result> SendPhoneOtpAsync(string phone, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Generate 6-digit OTP
        var code = GenerateOtpCode();
        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.Set<OtpCode>().Add(otp);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send SMS with OTP code via SMS service

        return Result.Succeed();
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
