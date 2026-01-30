namespace Clinic.Infrastructure.Abstractions;

public class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; }
    public string? CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
