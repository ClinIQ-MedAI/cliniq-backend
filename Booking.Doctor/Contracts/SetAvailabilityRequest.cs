namespace Booking.Doctor.Contracts;

public record AvailabilityDto(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
);

public record SetAvailabilityRequest(
    List<AvailabilityDto> Availabilities
);
