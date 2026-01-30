namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Doctor profile entity with Shared Primary Key pattern.
/// The Id is both the PK and a FK to ApplicationUser.Id.
/// </summary>
public class DoctorProfile
{
    /// <summary>
    /// Primary Key that is also the Foreign Key to ApplicationUser.Id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the doctor profile (verification workflow)
    /// </summary>
    public DoctorStatus Status { get; set; } = DoctorStatus.PENDING_VERIFICATION;

    // Doctor-specific properties
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }

    // Document URLs (stored in blob/file storage)
    public string? PersonalIdentityPhotoUrl { get; set; }
    public string? MedicalLicenseUrl { get; set; }

    // Rejection reason (if rejected by dashboard)
    public string? RejectionReason { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
