using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Entities;

/// <summary>
/// OTP code entity for one-time password authentication.
/// </summary>
public class OtpCode : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
