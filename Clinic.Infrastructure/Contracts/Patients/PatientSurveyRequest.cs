namespace Clinic.Infrastructure.Contracts.Patients;

/// <summary>
/// Survey request for patient profile creation.
/// Contains general health information.
/// </summary>
public record PatientSurveyRequest(
    decimal? Height,           // in cm
    decimal? Weight,           // in kg
    bool HasDiabetes,
    bool HasPressureIssues,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
