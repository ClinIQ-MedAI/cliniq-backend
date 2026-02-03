using Booking.Doctor.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.Doctor.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _dbContext;

    public ScheduleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SetAvailabilityAsync(string doctorId, SetAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var existingAvailabilities = await _dbContext.DoctorAvailabilities
            .Where(da => da.DoctorId == doctorId)
            .ToListAsync(cancellationToken);

        _dbContext.DoctorAvailabilities.RemoveRange(existingAvailabilities);

        foreach (var item in request.Availabilities)
        {
            var availability = new DoctorAvailability
            {
                DoctorId = doctorId,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                IsAvailable = true
            };
            _dbContext.DoctorAvailabilities.Add(availability);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task GenerateSchedulesAsync(string doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var availabilities = await _dbContext.DoctorAvailabilities
            .Where(da => da.DoctorId == doctorId && da.IsAvailable)
            .ToListAsync(cancellationToken);

        var existingSchedules = await _dbContext.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && ds.Date >= startDate && ds.Date <= endDate)
            .ToDictionaryAsync(ds => ds.Date, cancellationToken);

        var schedulesToAdd = new List<DoctorSchedule>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (existingSchedules.ContainsKey(date)) continue;

            var dayAvailability = availabilities.FirstOrDefault(a => a.DayOfWeek == date.DayOfWeek);
            if (dayAvailability != null)
            {
                schedulesToAdd.Add(new DoctorSchedule
                {
                    DoctorId = doctorId,
                    Date = date,
                    IsAvailable = true,
                    BookingCount = 0
                });
            }
        }

        if (schedulesToAdd.Any())
        {
            _dbContext.DoctorSchedules.AddRange(schedulesToAdd);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<DoctorSchedule>> GetSchedulesAsync(string doctorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DoctorSchedules.AsQueryable();

        query = query.Where(ds => ds.DoctorId == doctorId);

        if (from.HasValue)
            query = query.Where(ds => ds.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(ds => ds.Date <= to.Value);

        return await query.OrderBy(ds => ds.Date).ToListAsync(cancellationToken);
    }
}
