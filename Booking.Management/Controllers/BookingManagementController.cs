using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.Management.Controllers;

[ApiController]
[Route("admin/bookings")]
[Authorize(Roles = "Admin")]
public class BookingManagementController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public BookingManagementController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Bookings
            .Include(b => b.Patient)
                .ThenInclude(p => p.User)
            .Include(b => b.DoctorSchedule)
                .ThenInclude(ds => ds.Doctor)
                    .ThenInclude(d => d.User)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderByDescending(b => b.DoctorSchedule.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                b.Id,
                Date = b.DoctorSchedule.Date,
                PatientName = $"{b.Patient.User.FirstName} {b.Patient.User.LastName}",
                DoctorName = $"{b.DoctorSchedule.Doctor.User.FirstName} {b.DoctorSchedule.Doctor.User.LastName}",
                b.Status
            })
            .ToListAsync(cancellationToken);

        return Ok(new { Total = total, Data = bookings });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(int id, CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Patient)
                .ThenInclude(p => p.User)
            .Include(b => b.DoctorSchedule)
                .ThenInclude(ds => ds.Doctor)
                    .ThenInclude(d => d.User)
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.Status,
                Date = b.DoctorSchedule.Date,
                Patient = new
                {
                    b.Patient.User.Id,
                    b.Patient.User.FirstName,
                    b.Patient.User.LastName,
                    b.Patient.User.Email,
                    b.Patient.User.PhoneNumber
                },
                Doctor = new
                {
                    b.DoctorSchedule.Doctor.User.Id,
                    b.DoctorSchedule.Doctor.User.FirstName,
                    b.DoctorSchedule.Doctor.User.LastName,
                    b.DoctorSchedule.Doctor.Specialization
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
            return NotFound(Error.NotFound("Booking.NotFound", "Booking not found"));

        return Ok(booking);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings.FindAsync([id], cancellationToken);

        if (booking is null)
            return NotFound(Error.NotFound("Booking.NotFound", "Booking not found"));

        booking.Status = request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public record UpdateBookingStatusRequest(BookingStatus Status);
