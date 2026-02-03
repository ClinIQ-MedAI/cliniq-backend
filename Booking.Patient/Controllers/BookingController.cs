using Booking.Patient.Contracts;
using Booking.Patient.Services;

namespace Booking.Patient.Controllers;

[ApiController]
[Route("bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> BookAppointment([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {

        // UserExtensions has GetUserId. Do we track PatientId in claims? Probably not.
        // We likely need to lookup PatientProfile by UserId, or if the token has it.
        // Usually PatientId == UserId in Shared PK, so GetUserId() works if we use that ID.
        // Let's check PatientProfile.cs again. It uses Shared PK. So PatientId == UserId.

        var userId = User.GetUserId();

        try
        {
            var bookingId = await _bookingService.BookAppointmentAsync(userId!, request.DoctorScheduleId, cancellationToken);
            return Ok(new { BookingId = bookingId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var bookings = await _bookingService.GetMyBookingsAsync(userId, cancellationToken);
        return Ok(bookings);
    }
}

[ApiController]
[Route("api/doctors")]
public class DoctorLookupController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public DoctorLookupController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{doctorId}/schedules")]
    public async Task<IActionResult> GetDoctorSchedules(string doctorId, CancellationToken cancellationToken)
    {
        var schedules = await _bookingService.GetDoctorSchedulesAsync(doctorId, cancellationToken);
        return Ok(schedules);
    }
}
