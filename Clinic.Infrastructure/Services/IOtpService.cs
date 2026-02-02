namespace Clinic.Infrastructure.Services;

/// <summary>
/// Context for OTP generation and validation.
/// Determines the cache key prefix for isolation.
/// </summary>
public enum OtpContext
{
    /// <summary>Email or phone verification</summary>
    Verification,

    /// <summary>OTP-based login</summary>
    Login,

    /// <summary>Password reset</summary>
    ResetPassword
}

/// <summary>
/// Identifier type for OTP operations.
/// </summary>
public enum OtpIdentifierType
{
    Email,
    Phone
}

/// <summary>
/// Centralized OTP service for generation, storage, and validation.
/// Uses standardized cache key format: otp:{context}:{identifierType}:{value}
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generate and store an OTP for the given context and identifier.
    /// Returns the generated OTP code.
    /// </summary>
    Task<string> GenerateAndStoreAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate an OTP code. Does NOT consume the OTP.
    /// Use InvalidateAsync after successful validation if needed.
    /// </summary>
    Task<bool> ValidateAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate and consume an OTP in one operation.
    /// OTP is removed from cache if valid.
    /// </summary>
    Task<bool> ValidateAndConsumeAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate (remove) an OTP from cache.
    /// </summary>
    Task InvalidateAsync(
        OtpContext context,
        OtpIdentifierType identifierType,
        string identifier,
        CancellationToken cancellationToken = default);
}
