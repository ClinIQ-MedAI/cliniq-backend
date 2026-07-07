namespace Clinic.Authentication.Contracts;

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User,
    DoctorInfo? Doctor,
    PatientInfo? Patient,
    List<string> Roles
);

public record UserInfo(
    string Id,
    string Email,
    string? UserName,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed
);

public record DoctorInfo(
    string Status,
    string? Specialization,
    string? LicenseNumber,
    DateTime? LicenseExpiryDate,
    string? PersonalIdentityPhotoUrl,
    string? MedicalLicenseUrl,
    string? RejectionReason
);

public record PatientInfo(
    string Status,
    decimal? Height,
    decimal? Weight,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
