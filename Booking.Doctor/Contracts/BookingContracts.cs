using Clinic.Infrastructure.Entities.Enums;

namespace Booking.Doctor.Contracts;

public record DoctorBookingResponse(
    int Id,
    string PatientId,
    string PatientName,
    string? PatientEmail,
    string? PatientPhone,
    DateOnly Date,
    BookingStatus Status
);

public record BookingDetailResponse(
    int Id,
    string PatientId,
    string PatientName,
    string? PatientEmail,
    string? PatientPhone,
    DateOnly? PatientDateOfBirth,
    string? PatientGender,
    decimal? PatientHeight,
    decimal? PatientWeight,
    bool HasDiabetes,
    bool HasPressureIssues,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    DateOnly Date,
    BookingStatus Status
);

public record PatientAppointmentDto(
    int Id,
    DateOnly Date,
    BookingStatus Status
);

public record PatientDetailResponse(
    string PatientId,
    string PatientName,
    string? PatientEmail,
    string? PatientPhone,
    DateOnly? PatientDateOfBirth,
    string? PatientGender,
    decimal? PatientHeight,
    decimal? PatientWeight,
    bool HasDiabetes,
    bool HasPressureIssues,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    List<PatientAppointmentDto> Appointments
);

public record UpdateBookingStatusRequest(
    BookingStatus Status
);
