namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Status enum for doctor profiles.
/// </summary>
public enum DoctorStatus
{
    /// <summary>No DoctorProfile exists for this user</summary>
    INCOMPLETE_PROFILE,

    /// <summary>Awaiting dashboard approval</summary>
    PENDING_VERIFICATION,

    /// <summary>Dashboard rejected the verification</summary>
    REJECTED,

    /// <summary>Approved and active</summary>
    ACTIVE,

    /// <summary>Suspended by admin</summary>
    SUSPENDED
}
