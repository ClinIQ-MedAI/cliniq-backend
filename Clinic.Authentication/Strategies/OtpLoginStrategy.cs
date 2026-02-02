using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services;

namespace Clinic.Authentication.Strategies;

/// <summary>
/// Login strategy using OTP code.
/// </summary>
public class OtpLoginStrategy(IOtpService otpService) : ILoginStrategy
{
    private readonly IOtpService _otpService = otpService;

    public bool CanHandle(LoginRequest request)
    {
        return !string.IsNullOrEmpty(request.OtpCode);
    }

    public async Task<bool> ValidateAsync(ApplicationUser user, LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.OtpCode) || string.IsNullOrWhiteSpace(user.Email))
            return false;

        // Validate and consume OTP
        return await _otpService.ValidateAndConsumeAsync(
            OtpContext.Login,
            OtpIdentifierType.Email,
            user.Email,
            request.OtpCode,
            cancellationToken);
    }
}
