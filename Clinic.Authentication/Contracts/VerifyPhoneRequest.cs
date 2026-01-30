namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request model for phone verification with OTP code.
/// </summary>
public record VerifyPhoneRequest(
    string Phone,
    string Code
);
