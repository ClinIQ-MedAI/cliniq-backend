using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.Authentication.Services;

/// <summary>
/// Interface for password management (forgot/reset).
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Sends password reset code to email.
    /// </summary>
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets password using reset code.
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
