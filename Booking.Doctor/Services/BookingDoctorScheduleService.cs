using Booking.Doctor.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services;

namespace Booking.Doctor.Services;

public class BookingDoctorScheduleService : IBookingDoctorScheduleService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public BookingDoctorScheduleService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<Result> SetAvailabilityAsync(string doctorId, SetAvailabilityRequest request, CancellationToken cancellationToken = default)
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
                MaxBookings = item.MaxBookings,
                IsAvailable = true
            };
            _dbContext.DoctorAvailabilities.Add(availability);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);


        return Result.Succeed();
    }

    public async Task<Result> GenerateSchedulesAsync(string doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
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

        return Result.Succeed();
    }

    public async Task<Result<List<DoctorSchedule>>> GetSchedulesAsync(string doctorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DoctorSchedules.AsQueryable();

        query = query.Where(ds => ds.DoctorId == doctorId);

        if (from.HasValue)
            query = query.Where(ds => ds.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(ds => ds.Date <= to.Value);

        var schedules = await query.OrderBy(ds => ds.Date).ToListAsync(cancellationToken);

        return Result.Succeed(schedules);
    }

    public async Task<Result<List<DoctorBookingResponse>>> GetDoctorBookingsAsync(string doctorId, DateTime? from = null, DateTime? to = null, BookingStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Bookings
            .Include(b => b.DoctorSchedule)
            .Include(b => b.Patient)
                .ThenInclude(p => p.User)
            .Where(b => b.DoctorSchedule.DoctorId == doctorId)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(b => b.DoctorSchedule.Date >= DateOnly.FromDateTime(from.Value));

        if (to.HasValue)
            query = query.Where(b => b.DoctorSchedule.Date <= DateOnly.FromDateTime(to.Value));

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        var bookings = await query
            .OrderByDescending(b => b.DoctorSchedule.Date)
            .Select(b => new DoctorBookingResponse(
                b.Id,
                b.PatientId,
                $"{b.Patient.User.FirstName} {b.Patient.User.LastName}",
                b.Patient.User.Email,
                b.Patient.User.PhoneNumber,
                b.DoctorSchedule.Date,
                b.Status
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Succeed(bookings);
    }

    public async Task<Result<BookingDetailResponse>> GetBookingByIdAsync(string doctorId, int bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.DoctorSchedule)
            .Include(b => b.Patient)
                .ThenInclude(p => p.User)
            .Where(b => b.DoctorSchedule.DoctorId == doctorId)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("Booking.NotFound", "Booking not found"));

        var response = new BookingDetailResponse(
            booking.Id,
            booking.PatientId,
            $"{booking.Patient.User.FirstName} {booking.Patient.User.LastName}",
            booking.Patient.User.Email,
            booking.Patient.User.PhoneNumber,
            booking.Patient.User.DateOfBirth,
            booking.Patient.User.Gender?.ToString(),
            booking.Patient.Height,
            booking.Patient.Weight,
            booking.Patient.HasDiabetes,
            booking.Patient.HasPressureIssues,
            booking.Patient.BloodType,
            booking.Patient.Allergies,
            booking.Patient.ChronicConditions,
            booking.Patient.EmergencyContactName,
            booking.Patient.EmergencyContactPhone,
            booking.DoctorSchedule.Date,
            booking.Status
        );

        return Result.Succeed(response);
    }

    public async Task<Result<PatientDetailResponse>> GetPatientDetailAsync(string doctorId, string patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        if (patient is null)
            return Result.Failure<PatientDetailResponse>(Error.NotFound("Patient.NotFound", "Patient not found"));

        var appointments = await _dbContext.Bookings
            .Include(b => b.DoctorSchedule)
            .Where(b => b.PatientId == patientId && b.DoctorSchedule.DoctorId == doctorId)
            .OrderByDescending(b => b.DoctorSchedule.Date)
            .Select(b => new PatientAppointmentDto(
                b.Id,
                b.DoctorSchedule.Date,
                b.Status
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = new PatientDetailResponse(
            patient.Id,
            $"{patient.User.FirstName} {patient.User.LastName}",
            patient.User.Email,
            patient.User.PhoneNumber,
            patient.User.DateOfBirth,
            patient.User.Gender?.ToString(),
            patient.Height,
            patient.Weight,
            patient.HasDiabetes,
            patient.HasPressureIssues,
            patient.BloodType,
            patient.Allergies,
            patient.ChronicConditions,
            patient.EmergencyContactName,
            patient.EmergencyContactPhone,
            appointments
        );

        return Result.Succeed(response);
    }

    public async Task<Result> UpdateBookingStatusAsync(string doctorId, int bookingId, UpdateBookingStatusRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.DoctorSchedule)
            .Include(b => b.Patient)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.DoctorSchedule.DoctorId == doctorId, cancellationToken);

        if (booking is null)
            return Result.Failure(Error.NotFound("Booking.NotFound", "Booking not found"));

        var previousStatus = booking.Status;
        booking.Status = request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.Status == BookingStatus.CONFIRMED && previousStatus != BookingStatus.CONFIRMED)
        {
            await _notificationService.CreateNotificationAsync(
                "Appointment Confirmed",
                $"Your appointment on {booking.DoctorSchedule.Date:yyyy-MM-dd} has been confirmed.",
                NotificationType.BOOKING_CONFIRMED,
                [booking.Patient.User.Id],
                booking.Id.ToString()
            );
        }
        else if (request.Status == BookingStatus.CANCELLED && previousStatus != BookingStatus.CANCELLED)
        {
            await _notificationService.CreateNotificationAsync(
                "Appointment Cancelled",
                $"Your appointment on {booking.DoctorSchedule.Date:yyyy-MM-dd} has been cancelled.",
                NotificationType.BOOKING_CANCELLED,
                [booking.Patient.User.Id],
                booking.Id.ToString()
            );
        }
        else if (request.Status == BookingStatus.COMPLETED && previousStatus != BookingStatus.COMPLETED)
        {
            await _notificationService.CreateNotificationAsync(
                "Appointment Completed",
                $"Your appointment on {booking.DoctorSchedule.Date:yyyy-MM-dd} has been marked as completed.",
                NotificationType.BOOKING_COMPLETED,
                [booking.Patient.User.Id],
                booking.Id.ToString()
            );
        }

        return Result.Succeed();
    }
}
