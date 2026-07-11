namespace Clinic.Infrastructure.Contracts.Doctors;

public record SubmitDoctorUpdateRequest(
    string? Specialization,
    string? LicenseNumber,
    DateTime? LicenseExpiryDate,
    string? PersonalIdentityPhotoUrl,
    string? MedicalLicenseUrl
);
