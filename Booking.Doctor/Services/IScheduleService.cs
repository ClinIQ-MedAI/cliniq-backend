using Booking.Doctor.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;

namespace Booking.Doctor.Services;

public interface IScheduleService
{
    Task<Result> SetAvailabilityAsync(string doctorId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<Result> GenerateSchedulesAsync(string doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<Result<List<DoctorSchedule>>> GetSchedulesAsync(string doctorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
