namespace Clinic.Authentication.Contracts;

/// <summary>
/// Response returned after successful authentication.
/// </summary>
public record AuthResponse(
    string Id,
    string? Email,
    string FirstName,
    string LastName,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);
