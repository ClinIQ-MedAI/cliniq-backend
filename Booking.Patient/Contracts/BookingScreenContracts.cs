using Clinic.Infrastructure.Entities.Enums;

namespace Booking.Patient.Contracts;

public record BookingScreenViewModel(
    DoctorProfileDto Doctor,
    List<WorkingHoursDto> WorkingHours,
    List<CalendarSlotDto> Calendar
);

public record DoctorProfileDto(
    string Id,
    string Name,
    string Specialization,
    string? ImageUrl,
    double Rating,
    int ReviewCount,
    string? About
);

public record WorkingHoursDto(
    string Day,
    TimeSpan StartTime,
    TimeSpan EndTime
);

public record CalendarSlotDto(
    DateOnly Date,
    string DayName,
    int BookingCount,
    int MaxBookings,
    string Status // "Available", "Full", "Closed"
)
{
    public bool IsAvailable => Status == "Available";
}

public record DoctorSearchDto(
    string Id,
    string Name,
    string Specialization,
    string? ImageUrl,
    double Rating,
    int ReviewCount,
    TimeSpan StartTime,
    TimeSpan EndTime
);
