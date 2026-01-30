namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request model for forgot password - sends reset code.
/// </summary>
public record ForgotPasswordRequest(
    string Email
);
