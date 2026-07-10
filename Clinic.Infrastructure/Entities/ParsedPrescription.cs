using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Entities;

public class ParsedPrescription : AuditableEntity
{
    public int Id { get; set; }
    
    public string PatientId { get; set; } = string.Empty;
    public PatientProfile Patient { get; set; } = null!;
    
    // AI Integration
    public string? AIJobId { get; set; }
    public AIJob? AIJob { get; set; }
    
    public string? PrescriptionImageUrl { get; set; }
    public string? PrescriptionImageBase64 { get; set; }
    
    public string? RawParsedText { get; set; } // Raw AI response
    public string? MedicationsJson { get; set; } // Extracted structured array of medications (name, dosage, frequency)
    
    // Doctor Issuer/Reviewer (optional)
    public string? DoctorId { get; set; }
    public DoctorProfile? Doctor { get; set; }
    public string? DoctorNotes { get; set; }
}
