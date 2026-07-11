namespace Clinic.Infrastructure.Contracts.Doctors;

public record SubmitDoctorUpdateRequestRequest(
    string? Specialization,
    string? LicenseNumber,
    DateTime? LicenseExpiryDate,
    string? PersonalIdentityPhotoUrl,
    string? MedicalLicenseUrl
);
