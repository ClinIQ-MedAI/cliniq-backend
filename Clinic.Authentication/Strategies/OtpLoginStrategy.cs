using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authentication.Strategies;

/// <summary>
/// Login strategy using OTP code.
/// </summary>
public class OtpLoginStrategy(ICacheService cacheService) : ILoginStrategy
{
    private readonly ICacheService _cacheService = cacheService;
    private const string OtpPrefix = "otp:email:";

    public bool CanHandle(LoginRequest request)
    {
        return !string.IsNullOrEmpty(request.OtpCode);
    }

    public async Task<bool> ValidateAsync(ApplicationUser user, LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.OtpCode) || string.IsNullOrWhiteSpace(user.Email))
            return false;

        // Verify OTP from Redis
        var cacheKey = $"{OtpPrefix}{user.Email}";
        var storedCode = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

        if (storedCode == null || storedCode != request.OtpCode)
            return false;

        // Mark OTP as used (remove it)
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return true;
    }
}
