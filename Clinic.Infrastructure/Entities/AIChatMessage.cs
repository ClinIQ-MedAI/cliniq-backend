using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Entities;

public class AIChatMessage : AuditableEntity
{
    public int Id { get; set; }
    public string ChatId { get; set; } = string.Empty; // UUID hex string, generated on submission
    public string PatientId { get; set; } = string.Empty;
    public PatientProfile Patient { get; set; } = null!;
    
    public string Message { get; set; } = string.Empty;
    public string LanguagePreference { get; set; } = "ar";
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    
    public string? Reply { get; set; }
    public string? QueryType { get; set; }
    public bool ShowUpload { get; set; }
    public string? Error { get; set; }
    
    public string? Worker { get; set; }
    public double? DurationMs { get; set; }
    public DateTime? FinishedAt { get; set; }
}
