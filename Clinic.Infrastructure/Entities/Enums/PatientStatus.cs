using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Entities.Enums;

/// <summary>
/// Status enum for patient profiles.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatientStatus
{
    /// <summary>No PatientProfile exists for this user</summary>
    INCOMPLETE_PROFILE,

    /// <summary>Profile completed and active</summary>
    ACTIVE,

    /// <summary>Suspended by admin</summary>
    SUSPENDED
}
