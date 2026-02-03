using Booking.Doctor.Contracts;
using Clinic.Infrastructure.Entities;

namespace Booking.Doctor.Services;

public interface IScheduleService
{
    Task SetAvailabilityAsync(string doctorId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task GenerateSchedulesAsync(string doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<List<DoctorSchedule>> GetSchedulesAsync(string doctorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
