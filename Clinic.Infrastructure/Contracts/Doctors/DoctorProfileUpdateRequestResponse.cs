using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Contracts.Doctors;

public record DoctorProfileUpdateRequestResponse(
    int Id,
    string DoctorId,
    DoctorProfileUpdateRequestStatus Status,
    string? RejectionReason,
    string? Specialization,
    string? LicenseNumber,
    DateTime? LicenseExpiryDate,
    string? PersonalIdentityPhotoUrl,
    string? MedicalLicenseUrl,
    DateTime CreatedAt
);
