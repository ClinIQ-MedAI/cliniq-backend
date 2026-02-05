using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.Authentication.Services;

/// <summary>
/// Interface for authentication/login service.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns JWT.
    /// </summary>
    Task<Result<AuthTokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends OTP code to email for login.
    /// </summary>
    Task<Result> SendLoginOtpAsync(string email, CancellationToken cancellationToken = default);
}
