using Clinic.Infrastructure.Entities.Enums;

namespace Booking.Doctor.Contracts;

public record DoctorBookingResponse(
    int Id,
    string PatientName,
    string? PatientPhone,
    DateOnly Date,
    BookingStatus Status
);

public record UpdateBookingStatusRequest(
    BookingStatus Status
);
