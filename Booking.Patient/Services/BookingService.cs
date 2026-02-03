using Booking.Patient.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.Patient.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _dbContext;

    public BookingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> BookAppointmentAsync(string patientId, int scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _dbContext.DoctorSchedules
            .FirstOrDefaultAsync(ds => ds.Id == scheduleId, cancellationToken);

        if (schedule == null)
            throw new Exception("Schedule not found");

        if (!schedule.IsAvailable)
            throw new Exception("Schedule is not available");

        // Logic check for capacity could go here if MaxPatients existed.

        var booking = new Clinic.Infrastructure.Entities.Booking
        {
            PatientId = patientId,
            DoctorScheduleId = scheduleId,
            Status = BookingStatus.PENDING
        };

        _dbContext.Bookings.Add(booking);

        schedule.BookingCount++;
        // If we want to auto-close availability at 10, we could do it here
        // if (schedule.BookingCount >= 10) schedule.IsAvailable = false; 

        await _dbContext.SaveChangesAsync(cancellationToken);

        return booking.Id;
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(string patientId, CancellationToken cancellationToken = default)
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.DoctorSchedule)
                .ThenInclude(ds => ds.Doctor)
                    .ThenInclude(d => d.User) // To get name
            .Where(b => b.PatientId == patientId)
            .OrderByDescending(b => b.DoctorSchedule.Date)
            .Select(b => new BookingResponse(
                b.Id,
                $"{b.DoctorSchedule.Doctor.User.FirstName} {b.DoctorSchedule.Doctor.User.LastName}",
                b.DoctorSchedule.Date,
                b.Status
            ))
            .ToListAsync(cancellationToken);

        return bookings;
    }

    public async Task<List<DoctorScheduleDto>> GetDoctorSchedulesAsync(string doctorId, CancellationToken cancellationToken = default)
    {
        var schedules = await _dbContext.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && ds.IsAvailable)
            .OrderBy(ds => ds.Date)
            .Select(ds => new DoctorScheduleDto(
                ds.Id,
                ds.Date,
                ds.BookingCount,
                ds.IsAvailable
            ))
            .ToListAsync(cancellationToken);

        return schedules;
    }
}
