using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Entities;

public class DoctorProfileUpdateRequest : AuditableEntity
{
    public int Id { get; set; }

    public string DoctorId { get; set; } = string.Empty;
    public DoctorProfile Doctor { get; set; } = null!;

    public DoctorProfileUpdateRequestStatus Status { get; set; } = DoctorProfileUpdateRequestStatus.PENDING;
    public string? RejectionReason { get; set; }

    // Pending changes
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PersonalIdentityPhotoUrl { get; set; }
    public string? MedicalLicenseUrl { get; set; }
}
