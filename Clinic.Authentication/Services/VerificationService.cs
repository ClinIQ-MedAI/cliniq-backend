using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services;

namespace Clinic.Authentication.Services;

public class VerificationService(
    IOtpService otpService,
    UserManager<ApplicationUser> userManager) : IVerificationService
{
    private readonly IOtpService _otpService = otpService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result> SendEmailOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.EmailConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Email already verified"));

        // Generate and store OTP
        await _otpService.GenerateAndStoreAsync(
            OtpContext.Verification,
            OtpIdentifierType.Email,
            email,
            cancellationToken);

        // TODO: Send email

        return Result.Succeed();
    }

    public async Task<Result> SendPhoneOtpAsync(string phone, CancellationToken cancellationToken = default)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phone);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.PhoneNumberConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Phone already verified"));

        // Generate and store OTP
        await _otpService.GenerateAndStoreAsync(
            OtpContext.Verification,
            OtpIdentifierType.Phone,
            phone,
            cancellationToken);

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

        // Validate and consume OTP
        var isValid = await _otpService.ValidateAndConsumeAsync(
            OtpContext.Verification,
            OtpIdentifierType.Email,
            request.Email,
            request.Code,
            cancellationToken);

        if (!isValid)
        {
            return Result.Failure(new Error("Auth.InvalidOtp", "Invalid or expired OTP", StatusCodes.Status400BadRequest));
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Result.Succeed();
    }

    public async Task<Result> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken = default)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == request.Phone);
        if (user == null)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        if (user.PhoneNumberConfirmed)
            return Result.Failure(Error.Conflict("User.AlreadyVerified", "Phone already verified"));

        // Validate and consume OTP
        var isValid = await _otpService.ValidateAndConsumeAsync(
            OtpContext.Verification,
            OtpIdentifierType.Phone,
            request.Phone,
            request.Code,
            cancellationToken);

        if (!isValid)
        {
            return Result.Failure(new Error("Auth.InvalidOtp", "Invalid or expired OTP", StatusCodes.Status400BadRequest));
        }

        user.PhoneNumberConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Result.Succeed();
    }
}
