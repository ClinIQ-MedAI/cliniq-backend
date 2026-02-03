using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.Management.Controllers;

[ApiController]
[Route("admin/bookings")]
[Authorize(Roles = "Admin")] // Assuming Admin role exists or policy
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
}
