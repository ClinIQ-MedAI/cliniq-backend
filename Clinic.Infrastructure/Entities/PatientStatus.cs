namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Status enum for patient profiles.
/// </summary>
public enum PatientStatus
{
    /// <summary>No PatientProfile exists for this user</summary>
    INCOMPLETE_PROFILE,

    /// <summary>Profile completed and active</summary>
    ACTIVE,

    /// <summary>Suspended by admin</summary>
    SUSPENDED
}
