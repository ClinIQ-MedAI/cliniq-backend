namespace Clinic.Authentication.Contracts.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    // Patient survey fields
    decimal? Height,
    decimal? Weight,
    bool? HasDiabetes,
    bool? HasPressureIssues,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
