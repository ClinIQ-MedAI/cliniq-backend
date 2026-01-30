using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Services;

public interface IVerificationService
{
    Task<Result> SendEmailOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> VerifyEmailOtpAsync(string email, string code, CancellationToken cancellationToken = default);

    // Future: Phone verification
    // Task<Result> SendPhoneOtpAsync(string phone, CancellationToken cancellationToken = default);
    // Task<Result> VerifyPhoneOtpAsync(string phone, string code, CancellationToken cancellationToken = default);
}
