using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DoctorProfileUpdateRequestStatus
{
    PENDING,
    APPROVED,
    REJECTED
}
