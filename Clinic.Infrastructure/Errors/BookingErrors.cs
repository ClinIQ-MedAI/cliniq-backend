using Clinic.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Clinic.Infrastructure.Errors;

public static class BookingErrors
{
    public static readonly Error DoctorUnavailable = Error.Conflict("Booking.DoctorUnavailable", "Doctor is not available on this date");
    public static readonly Error ScheduleUnavailable = Error.Conflict("Booking.ScheduleUnavailable", "Schedule is not available");
    public static readonly Error BookingLimitExceeded = Error.Conflict("Booking.BookingLimitExceeded", "Doctor has reached maximum booking capacity for this date");
    public static readonly Error NotFound = Error.NotFound("Booking.ScheduleNotFound", "Schedule not found");
}
