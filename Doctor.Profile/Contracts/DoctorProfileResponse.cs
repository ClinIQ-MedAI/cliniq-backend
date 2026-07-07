namespace Doctor.Profile.Contracts;

public record DoctorProfileResponse(
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string? Specialization,
    string? LicenseNumber,
    DateTime? LicenseExpiryDate,
    string? PersonalIdentityPhotoUrl,
    string? MedicalLicenseUrl,
    string? RejectionReason,
    string Status
);
