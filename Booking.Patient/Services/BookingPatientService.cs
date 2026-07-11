using Booking.Patient.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services;
using Booking.Patient.Localization;
using Microsoft.Extensions.Localization;

namespace Booking.Patient.Services;

public class BookingPatientService : IBookingPatientService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly int UpcomingPeriodDays = 7;

    public BookingPatientService(AppDbContext dbContext,
        INotificationService notificationService,
        IStringLocalizer<Messages> localizer)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    private readonly IStringLocalizer<Messages> _localizer;

    public async Task<Result<int>> BookAppointmentAsync(string patientId, string doctorId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);

        var doctor = await _dbContext.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        var schedule = await _dbContext.DoctorSchedules
            .FirstOrDefaultAsync(ds => ds.DoctorId == doctorId && ds.Date == date, cancellationToken);

        var dayOfWeek = date.DayOfWeek;
        var availability = await _dbContext.DoctorAvailabilities
            .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.DayOfWeek == dayOfWeek, cancellationToken);

        if (availability == null || !availability.IsAvailable)
            return Result.Failure<int>(Error.Conflict("Booking.DoctorUnavailable", _localizer["DoctorUnavailable"]));

        if (schedule == null)
        {
            schedule = new DoctorSchedule
            {
                DoctorId = doctorId,
                Date = date,
                IsAvailable = true,
                BookingCount = 0
            };

            _dbContext.DoctorSchedules.Add(schedule);
        }
        else
        {
            if (!schedule.IsAvailable)
                return Result.Failure<int>(Error.Conflict("Booking.ScheduleUnavailable", _localizer["ScheduleUnavailable"]));
        }

        if (schedule.BookingCount >= availability.MaxBookings)
            return Result.Failure<int>(Error.Conflict("Booking.BookingLimitExceeded", _localizer["BookingLimitExceeded"]));

        var booking = new Clinic.Infrastructure.Entities.Booking
        {
            PatientId = patientId,
            DoctorSchedule = schedule,
            Status = BookingStatus.PENDING
        };

        _dbContext.Bookings.Add(booking);

        schedule.BookingCount++;

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (patient?.User is not null && doctor?.User is not null)
        {
            await _notificationService.CreateNotificationAsync(
                "New Appointment",
                $"You have a new booking from {patient.User.FirstName} {patient.User.LastName} on {date:yyyy-MM-dd}.",
                NotificationType.BOOKING_CREATED,
                [doctor.User.Id],
                booking.Id.ToString()
            );
        }

        return Result.Succeed(booking.Id);
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
            .AsNoTracking()
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

    public async Task<Result<BookingScreenViewModel>> GetBookingScreenDetailsAsync(string doctorId, CancellationToken cancellationToken = default)
    {
        // 1. Fetch Doctor Profile with his availability days
        var doctor = await _dbContext.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.AvailabilityDays)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor is null)
            return Result.Failure<BookingScreenViewModel>(Error.NotFound("Doctor.NotFound", _localizer["DoctorNotFound"]));

        // 2. Map Availability (Working Hours)
        var availabilities = doctor.AvailabilityDays.Where(a => a.IsAvailable).ToList();

        // 3. Fetch Existing Schedules for next UpcomingPeriodDays
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(UpcomingPeriodDays);

        var schedules = await _dbContext.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && ds.Date >= today && ds.Date <= endDate)
            .ToListAsync(cancellationToken);

        // 4. Map Working Hours
        var workingHoursDtos = availabilities.Select(a => new WorkingHoursDto(
            a.DayOfWeek.ToString(),
            a.StartTime,
            a.EndTime
        )).ToList();

        // Generate Calendar
        var calendar = new List<CalendarSlotDto>();
        for (int i = 0; i < UpcomingPeriodDays; i++)
        {
            var date = today.AddDays(i);
            var dayOfWeek = date.DayOfWeek;

            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
            var schedule = schedules.FirstOrDefault(s => s.Date == date);

            string status = "Closed";
            int maxBookings = 0;
            int currentBookings = schedule?.BookingCount ?? 0;

            if (availability != null)
            {
                maxBookings = availability.MaxBookings;

                if (schedule != null)
                {
                    // If schedule exists, check if it's explicitly available and not full
                    if (!schedule.IsAvailable)
                    {
                        status = "Closed"; // Or "Unavailable" if manually closed
                    }
                    else if (schedule.BookingCount >= maxBookings)
                    {
                        status = "Full";
                    }
                    else
                    {
                        status = "Available";
                    }
                }
                else
                {
                    // No schedule yet, but doctor is available on this day of week
                    status = "Available";
                }
            }

            calendar.Add(new CalendarSlotDto(
                date,
                date.DayOfWeek.ToString(),
                currentBookings,
                maxBookings,
                status
            ));
        }

        var profileDto = new DoctorProfileDto(
            doctor.Id,
            $"{doctor.User.FirstName} {doctor.User.LastName}",
            doctor.Specialization ?? "General",
            doctor.PersonalIdentityPhotoUrl, // Using this as image for now
            4.8, // Hardcoded Rating
            120, // Hardcoded Reviews
            "Dr. Safira is a highly skilled specialist with over 10 years of experience..." // Hardcoded About
        );

        return Result.Succeed(new BookingScreenViewModel(profileDto, workingHoursDtos, calendar));
    }

    public async Task<Result<List<DoctorSearchDto>>> GetAvailableDoctorsAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var dayOfWeek = date.DayOfWeek;

        var doctors = await _dbContext.DoctorProfiles
            .Include(d => d.User)
            .SelectMany(
                d => d.AvailabilityDays.Where(da => da.DayOfWeek == dayOfWeek && da.IsAvailable),
                (doctor, availability) => new DoctorSearchDto(
                    doctor.Id,
                    $"{doctor.User.FirstName} {doctor.User.LastName}",
                    doctor.Specialization ?? "General",
                    doctor.PersonalIdentityPhotoUrl,
                    4.8,
                    120,
                    availability.StartTime,
                    availability.EndTime
                ))
            .ToListAsync(cancellationToken);

        return Result.Succeed(doctors);
    }

    public async Task<List<FlutterAppointmentDto>> GetExaminationAppointmentsAsync(string patientId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bookings = await _dbContext.Bookings
            .Include(b => b.DoctorSchedule)
                .ThenInclude(ds => ds.Doctor)
                    .ThenInclude(d => d.User)
            .Include(b => b.DoctorSchedule)
                .ThenInclude(ds => ds.Doctor)
                    .ThenInclude(d => d.AvailabilityDays)
            .Where(b => b.PatientId == patientId
                && (b.Status == BookingStatus.PENDING || b.Status == BookingStatus.CONFIRMED)
                && b.DoctorSchedule.Date >= today)
            .OrderBy(b => b.DoctorSchedule.Date)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return bookings.Select(b =>
        {
            var doctor = b.DoctorSchedule.Doctor;
            var availability = doctor.AvailabilityDays
                .FirstOrDefault(a => a.DayOfWeek == b.DoctorSchedule.Date.DayOfWeek && a.IsAvailable);
            var time = availability?.StartTime ?? new TimeSpan(9, 0, 0);

            return new FlutterAppointmentDto(
                b.Id.ToString(),
                $"{doctor.User.FirstName} {doctor.User.LastName}",
                doctor.Specialization ?? "General",
                doctor.PersonalIdentityPhotoUrl ?? "",
                b.DoctorSchedule.Date.ToString("yyyy-MM-dd"),
                $"{time.Hours:D2}:{time.Minutes:D2}",
                "Upcoming"
            );
        }).ToList();
    }

    public async Task<List<FlutterAppointmentDto>> GetAvailableDoctorsFlutterAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var dayOfWeek = date.DayOfWeek;

        var doctors = await _dbContext.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.AvailabilityDays)
            .SelectMany(
                d => d.AvailabilityDays.Where(da => da.DayOfWeek == dayOfWeek && da.IsAvailable),
                (doctor, availability) => new FlutterAppointmentDto(
                    doctor.Id,
                    $"{doctor.User.FirstName} {doctor.User.LastName}",
                    doctor.Specialization ?? "General",
                    doctor.PersonalIdentityPhotoUrl ?? "",
                    date.ToString("yyyy-MM-dd"),
                    $"{availability.StartTime.Hours:D2}:{availability.StartTime.Minutes:D2}",
                    "Available"
                ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return doctors;
    }

    public async Task<Result<FlutterDoctorScheduleDataDto>> GetDoctorScheduleFlutterAsync(string doctorId, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.DoctorProfiles
            .Include(d => d.AvailabilityDays)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor is null)
            return Result.Failure<FlutterDoctorScheduleDataDto>(Error.NotFound("Doctor.NotFound", _localizer["DoctorNotFound"]));

        var availabilities = doctor.AvailabilityDays.Where(a => a.IsAvailable).ToList();

        var weeklySchedule = availabilities.Select(a => new FlutterWeeklyScheduleDto(
            a.DayOfWeek.ToString()[..3],
            $"{a.StartTime.Hours:D2}:{a.StartTime.Minutes:D2} - {a.EndTime.Hours:D2}:{a.EndTime.Minutes:D2}"
        )).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(UpcomingPeriodDays);

        var schedules = await _dbContext.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && ds.Date >= today && ds.Date <= endDate)
            .ToListAsync(cancellationToken);

        var dates = new List<FlutterScheduleDateDto>();
        for (int i = 0; i < UpcomingPeriodDays; i++)
        {
            var date = today.AddDays(i);
            var dayOfWeek = date.DayOfWeek;
            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
            var schedule = schedules.FirstOrDefault(s => s.Date == date);

            if (availability == null)
                continue;

            bool isFull;
            int currentBookings;
            int maxBookings = availability.MaxBookings;

            if (schedule != null)
            {
                currentBookings = schedule.BookingCount;
                isFull = !schedule.IsAvailable || schedule.BookingCount >= maxBookings;
            }
            else
            {
                currentBookings = 0;
                isFull = false;
            }

            dates.Add(new FlutterScheduleDateDto(
                dayOfWeek.ToString()[..3],
                date.Day.ToString(),
                date.ToString("MMM"),
                date.ToString("yyyy-MM-dd"),
                isFull ? "Full" : $"{currentBookings}/{maxBookings}",
                isFull
            ));
        }

        return Result.Succeed(new FlutterDoctorScheduleDataDto(weeklySchedule, dates));
    }

    public async Task<Result<FlutterDoctorDetailsDataDto>> GetDoctorDetailsFlutterAsync(string doctorId, CancellationToken cancellationToken = default)
    {
        var doctor = await _dbContext.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.AvailabilityDays)
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor is null)
            return Result.Failure<FlutterDoctorDetailsDataDto>(Error.NotFound("Doctor.NotFound", _localizer["DoctorNotFound"]));

        var totalAppointments = await _dbContext.Bookings
            .CountAsync(b => b.DoctorSchedule.DoctorId == doctorId && b.Status != BookingStatus.CANCELLED, cancellationToken);

        var availabilities = doctor.AvailabilityDays.Where(a => a.IsAvailable).ToList();

        var weeklySchedule = availabilities.Select(a => new FlutterWeeklyScheduleDto(
            a.DayOfWeek.ToString()[..3],
            $"{a.StartTime.Hours:D2}:{a.StartTime.Minutes:D2} - {a.EndTime.Hours:D2}:{a.EndTime.Minutes:D2}"
        )).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(UpcomingPeriodDays);

        var schedules = await _dbContext.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && ds.Date >= today && ds.Date <= endDate)
            .ToListAsync(cancellationToken);

        var dates = new List<FlutterScheduleDateDto>();
        for (int i = 0; i < UpcomingPeriodDays; i++)
        {
            var date = today.AddDays(i);
            var dayOfWeek = date.DayOfWeek;
            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
            var schedule = schedules.FirstOrDefault(s => s.Date == date);

            if (availability == null)
                continue;

            bool isFull;
            int currentBookings;
            int maxBookings = availability.MaxBookings;

            if (schedule != null)
            {
                currentBookings = schedule.BookingCount;
                isFull = !schedule.IsAvailable || schedule.BookingCount >= maxBookings;
            }
            else
            {
                currentBookings = 0;
                isFull = false;
            }

            dates.Add(new FlutterScheduleDateDto(
                dayOfWeek.ToString()[..3],
                date.Day.ToString(),
                date.ToString("MMM"),
                date.ToString("yyyy-MM-dd"),
                isFull ? "Full" : $"{currentBookings}/{maxBookings}",
                isFull
            ));
        }

        var doctorDto = new FlutterDoctorDetailsDto(
            doctor.Id,
            $"{doctor.User.FirstName} {doctor.User.LastName}",
            doctor.PersonalIdentityPhotoUrl ?? "",
            doctor.Specialization ?? "General",
            "10 years",
            "4.5",
            totalAppointments.ToString(),
            "Cairo"
        );

        return Result.Succeed(new FlutterDoctorDetailsDataDto(
            doctorDto,
            new FlutterDoctorScheduleDataDto(weeklySchedule, dates)
        ));
    }
}
