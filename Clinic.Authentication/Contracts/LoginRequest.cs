namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request model for user login.
/// </summary>
public record LoginRequest(
    string Email,
    string? Password,
    string? OtpCode
);
