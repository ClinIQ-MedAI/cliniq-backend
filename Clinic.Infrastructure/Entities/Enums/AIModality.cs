using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AIModality
{
    BONE,
    DENTAL_XRAY,
    DENTAL_PHOTO,
    CHEST,
    PRESCRIPTION
}
