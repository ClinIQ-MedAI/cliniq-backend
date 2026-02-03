using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    PENDING,
    CONFIRMED,
    CANCELLED,
    COMPLETED
}
