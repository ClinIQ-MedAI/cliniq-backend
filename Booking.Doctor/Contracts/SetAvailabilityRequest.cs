using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Booking.Doctor.Contracts;

public record AvailabilityDto(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    [Range(1, 100, ErrorMessage = "Validation.MaxBookingsRange")] int MaxBookings
);

public record SetAvailabilityRequest(
    List<AvailabilityDto> Availabilities
);
