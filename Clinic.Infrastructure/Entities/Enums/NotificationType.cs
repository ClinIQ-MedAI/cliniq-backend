using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    BOOKING_CREATED,
    BOOKING_CONFIRMED,
    BOOKING_CANCELLED,
    BOOKING_COMPLETED,
    DOCTOR_JOIN_REQUEST,
    DOCTOR_PROFILE_UPDATE_REQUEST,
    PATIENT_NEW_REGISTRATION,
    CONTACT_US_MESSAGE,
    ADMIN_BROADCAST
}
