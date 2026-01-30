using Clinic.Infrastructure.Entities;

namespace Clinic.Authentication.Jwt;

/// <summary>
/// Interface for JWT token generation and validation.
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// Generates a JWT token with user claims.
    /// </summary>
    (string Token, int ExpiresIn) GenerateToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        PatientStatus patientStatus,
        DoctorStatus doctorStatus);

    /// <summary>
    /// Generates a JWT token asynchronously, loading roles from UserManager.
    /// </summary>
    Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(
        ApplicationUser user,
        PatientStatus patientStatus,
        DoctorStatus doctorStatus);

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns the user ID.
    /// </summary>
    string? ValidateToken(string token);
}
