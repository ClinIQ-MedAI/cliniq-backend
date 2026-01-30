using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinic.Authentication.Services;

/// <summary>
/// Password management service (forgot/reset).
/// </summary>
public class PasswordService(UserManager<ApplicationUser> userManager) : IPasswordService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return Result.Succeed();
        }

        // Generate password reset token
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: Send reset token to email
        // The token should be URL-encoded and sent via email service

        return Result.Succeed();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await _userManager.ResetPasswordAsync(user, request.Code, request.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Succeed();
    }
}
