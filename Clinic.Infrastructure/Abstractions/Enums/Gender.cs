using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Abstractions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male,
    Female
}
