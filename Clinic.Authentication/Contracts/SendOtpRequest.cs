namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request to send OTP code to email.
/// </summary>
public record SendEmailOtpRequest(string Email);

/// <summary>
/// Request to send OTP code to phone.
/// </summary>
public record SendPhoneOtpRequest(string Phone);
