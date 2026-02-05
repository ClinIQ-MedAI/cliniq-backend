namespace Clinic.Authentication.Authorization;

/// <summary>
/// Constants for authorization policy names.
/// </summary>
public static class PolicyNames
{
    /// <summary>
    /// Requires user to have verified email OR phone.
    /// </summary>
    public const string VerifiedUser = "VerifiedUser";

    /// <summary>
    /// Requires patient_status claim to not be INCOMPLETE_PROFILE.
    /// </summary>
    public const string PatientProfileRequired = "PatientProfileRequired";

    /// <summary>
    /// Requires doctor_status claim to not be INCOMPLETE_PROFILE.
    /// </summary>
    public const string DoctorProfileRequired = "DoctorProfileRequired";

    /// <summary>
    /// Requires patient_status to be ACTIVE.
    /// </summary>
    public const string ActivePatient = "ActivePatient";

    /// <summary>
    /// Requires doctor_status to be ACTIVE.
    /// </summary>
    public const string ActiveDoctor = "ActiveDoctor";

    /// <summary>
    /// Requires doctor_status to be PENDING_VERIFICATION.
    /// </summary>
    public const string PendingDoctor = "PendingDoctor";

    /// <summary>
    /// Requires Admin role.
    /// </summary>
    public const string Admin = "Admin";
}
