using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.Authentication.Services;

/// <summary>
/// Interface for email and phone verification using OTP.
/// </summary>
public interface IVerificationService
{
    /// <summary>
    /// Verifies email using OTP code.
    /// </summary>
    Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies phone using OTP code.
    /// </summary>
    Task<Result> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends OTP code to email for verification.
    /// </summary>
    Task<Result> SendEmailOtpAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends OTP code to phone for verification.
    /// </summary>
    Task<Result> SendPhoneOtpAsync(string phone, CancellationToken cancellationToken = default);
}
