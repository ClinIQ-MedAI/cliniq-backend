using Booking.Patient.Contracts;

namespace Booking.Patient.Services;

public interface IBookingService
{
    Task<int> BookAppointmentAsync(string patientId, int scheduleId, CancellationToken cancellationToken = default);
    Task<List<BookingResponse>> GetMyBookingsAsync(string patientId, CancellationToken cancellationToken = default);
    Task<List<DoctorScheduleDto>> GetDoctorSchedulesAsync(string doctorId, CancellationToken cancellationToken = default);
}
