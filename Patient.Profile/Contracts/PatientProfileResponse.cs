namespace Patient.Profile.Contracts;

public record PatientProfileResponse(
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? Gender,
    string Status,
    decimal? Height,
    decimal? Weight,
    bool HasDiabetes,
    bool HasPressureIssues,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
