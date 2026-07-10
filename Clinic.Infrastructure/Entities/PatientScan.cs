using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Entities;

public class PatientScan : AuditableEntity
{
    public int Id { get; set; }
    
    public string PatientId { get; set; } = string.Empty;
    public PatientProfile Patient { get; set; } = null!;
    
    public string Modality { get; set; } = string.Empty; // bone, chest, dental_xray, dental_photo
    public string? ScanUrl { get; set; }
    public string? ScanBase64 { get; set; }
    
    // AI Integration
    public string? AIJobId { get; set; }
    public AIJob? AIJob { get; set; }
    public string? AIAnalysisResult { get; set; } // JSON serialized payload
    
    // Doctor Review
    public string? DoctorId { get; set; }
    public DoctorProfile? Doctor { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime? DoctorReviewDate { get; set; }
    public bool IsReviewed { get; set; } = false;
}
