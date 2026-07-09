using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Entities;

public class AIJob : AuditableEntity
{
    public string Id { get; set; } = string.Empty; // UUID hex string, generated on submission
    public string Modality { get; set; } = string.Empty; // bone, dental_xray, chest, dental_photo, prescription
    public string PatientId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

    // Request Payloads
    public string? ImageBase64 { get; set; }
    public string? ImageUrl { get; set; }
    public string? OptionsJson { get; set; }
    public string? ReplyTo { get; set; }

    // Processing Result Payloads
    public string? ResultJson { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Worker { get; set; }
    public double? DurationMs { get; set; }
    public DateTime? FinishedAt { get; set; }
}
