using Clinic.Authentication.Contracts;

namespace Clinic.Authentication.Services;

/// <summary>
/// Interface for authentication/login service.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns JWT.
    /// </summary>
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
