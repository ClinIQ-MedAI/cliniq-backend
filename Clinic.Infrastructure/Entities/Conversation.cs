using Clinic.Infrastructure.Abstractions;

namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Represents a chat conversation between a doctor and a patient.
/// Each doctor-patient pair can have only one conversation.
/// </summary>
public class Conversation : AuditableEntity
{
    public int Id { get; set; }

    // Participants
    public string DoctorId { get; set; } = string.Empty;
    public DoctorProfile Doctor { get; set; } = null!;

    public string PatientId { get; set; } = string.Empty;
    public PatientProfile Patient { get; set; } = null!;

    // Last activity for sorting/display
    public DateTime? LastMessageAt { get; set; }

    // Navigation property
    public ICollection<Message> Messages { get; set; } = [];
}
