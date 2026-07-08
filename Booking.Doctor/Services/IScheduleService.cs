using Booking.Doctor.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;

namespace Booking.Doctor.Services;

public interface IScheduleService
{
    Task<Result> SetAvailabilityAsync(string doctorId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<Result> GenerateSchedulesAsync(string doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<Result<List<DoctorSchedule>>> GetSchedulesAsync(string doctorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<Result<List<DoctorBookingResponse>>> GetDoctorBookingsAsync(string doctorId, DateTime? from = null, DateTime? to = null, BookingStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<BookingDetailResponse>> GetBookingByIdAsync(string doctorId, int bookingId, CancellationToken cancellationToken = default);
    Task<Result<PatientDetailResponse>> GetPatientDetailAsync(string doctorId, string patientId, CancellationToken cancellationToken = default);
    Task<Result> UpdateBookingStatusAsync(string doctorId, int bookingId, UpdateBookingStatusRequest request, CancellationToken cancellationToken = default);
}
