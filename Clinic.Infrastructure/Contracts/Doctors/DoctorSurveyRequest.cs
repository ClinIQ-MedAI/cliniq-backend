namespace Clinic.Infrastructure.Contracts.Doctors;

/// <summary>
/// Survey request for doctor profile creation.
/// Submitted after email/phone verification.
/// </summary>
public record DoctorSurveyRequest(
    string PersonalIdentityPhotoUrl,
    string MedicalLicenseUrl,
    string Specialization,
    string LicenseNumber,
    DateTime? LicenseExpiryDate = null
);
