namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request model for email verification with OTP code.
/// </summary>
public record VerifyEmailRequest(
    string Email,
    string Code
);
