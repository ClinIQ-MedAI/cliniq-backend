using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Patient profile entity with Shared Primary Key pattern.
/// The Id is both the PK and a FK to ApplicationUser.Id.
/// </summary>
public class PatientProfile
{
    /// <summary>
    /// Primary Key that is also the Foreign Key to ApplicationUser.Id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the patient profile
    /// </summary>
    public PatientStatus Status { get; set; } = PatientStatus.ACTIVE;

    // Health survey fields
    public decimal? Height { get; set; }  // in cm
    public decimal? Weight { get; set; }  // in kg
    public bool HasDiabetes { get; set; }
    public bool HasPressureIssues { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }

    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
