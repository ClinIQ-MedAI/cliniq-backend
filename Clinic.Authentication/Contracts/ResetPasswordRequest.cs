namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request model for resetting password with reset code.
/// </summary>
public record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);
