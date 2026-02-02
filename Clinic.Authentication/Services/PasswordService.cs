using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services;

namespace Clinic.Authentication.Services;

/// <summary>
/// Password management service (forgot/reset).
/// Uses IOtpService for OTP operations.
/// </summary>
public class PasswordService(
    UserManager<ApplicationUser> userManager,
    IOtpService otpService) : IPasswordService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IOtpService _otpService = otpService;

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return Result.Succeed();
        }

        // Generate and store OTP
        await _otpService.GenerateAndStoreAsync(
            OtpContext.ResetPassword,
            OtpIdentifierType.Email,
            request.Email,
            cancellationToken);

        // TODO: Send OTP to email

        return Result.Succeed();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Validate and consume OTP
        var isValid = await _otpService.ValidateAndConsumeAsync(
            OtpContext.ResetPassword,
            OtpIdentifierType.Email,
            request.Email,
            request.Code,
            cancellationToken);

        if (!isValid)
        {
            return Result.Failure(new Error("InvalidToken", "Invalid or expired reset code.", StatusCodes.Status400BadRequest));
        }

        // Remove old password and set new one
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            var error = removeResult.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
        {
            var error = addResult.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Succeed();
    }
}
