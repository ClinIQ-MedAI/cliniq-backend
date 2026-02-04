using Clinic.Infrastructure.Entities.Enums;

namespace Booking.Patient.Contracts;

public record CreateBookingRequest(
    string DoctorId,
    DateOnly Date
);

public record BookingResponse(
    int Id,
    string DoctorName,
    DateOnly Date,
    BookingStatus Status
);

public record DoctorScheduleDto(
    int Id,
    DateOnly Date,
    int BookingCount,
    bool IsAvailable
);
