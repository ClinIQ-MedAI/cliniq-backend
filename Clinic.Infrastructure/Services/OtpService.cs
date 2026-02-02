using Microsoft.Extensions.Hosting;

namespace Clinic.Infrastructure.Services;

/// <summary>
/// OTP service implementation using cache for storage.
/// Generates fixed "12345" OTP in development, random 5-digit in production.
/// </summary>
public class OtpService(
    ICacheService cacheService,
    IHostEnvironment environment) : IOtpService
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly IHostEnvironment _environment = environment;

    private static readonly TimeSpan DefaultOtpTtl = TimeSpan.FromMinutes(10);

    public async Task<string> GenerateAndStoreAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var otp = GenerateOtp();
        var cacheKey = BuildCacheKey(context, identifierType, identifier);

        await _cacheService.SetAsync(cacheKey, otp, DefaultOtpTtl, cancellationToken);

        return otp;
    }

    public async Task<bool> ValidateAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        string code,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(context, identifierType, identifier);
        var storedOtp = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

        return !string.IsNullOrEmpty(storedOtp) && storedOtp == code;
    }

    public async Task<bool> ValidateAndConsumeAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        string code,
        CancellationToken cancellationToken = default)
    {
        var isValid = await ValidateAsync(context, identifierType, identifier, code, cancellationToken);

        if (isValid)
        {
            await InvalidateAsync(context, identifierType, identifier, cancellationToken);
        }

        return isValid;
    }

    public async Task InvalidateAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(context, identifierType, identifier);
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
    }

    private static string BuildCacheKey(OtpContext context, OtpIdentifierType identifierType, string identifier)
    {
        var contextStr = context switch
        {
            OtpContext.Verification => "verification",
            OtpContext.Login => "login",
            OtpContext.ResetPassword => "reset_password",
            _ => throw new ArgumentOutOfRangeException(nameof(context))
        };

        var identifierTypeStr = identifierType switch
        {
            OtpIdentifierType.Email => "email",
            OtpIdentifierType.Phone => "phone",
            _ => throw new ArgumentOutOfRangeException(nameof(identifierType))
        };

        return $"otp:{contextStr}:{identifierTypeStr}:{identifier}";
    }

    private string GenerateOtp()
    {
        if (_environment.IsDevelopment())
        {
            return "12345";
        }

        return Random.Shared.Next(10000, 99999).ToString();
    }
}
