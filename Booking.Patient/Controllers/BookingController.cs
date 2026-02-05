using Booking.Patient.Contracts;
using Booking.Patient.Services;
using Clinic.Authentication.Authorization;

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
    [Authorize(Policy = PolicyNames.ActivePatient)]
    public async Task<IActionResult> BookAppointment([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _bookingService.BookAppointmentAsync(userId!, request.DoctorId, request.Date, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.StatusCode switch
            {
                404 => NotFound(result.Error),
                409 => Conflict(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { BookingId = result.Value });
    }

    [HttpGet("me")]
    [Authorize(Policy = PolicyNames.ActivePatient)]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var bookings = await _bookingService.GetMyBookingsAsync(userId!, cancellationToken);

        return Ok(bookings);
    }    

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetAvailableDoctorsAsync(date, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("doctors/{doctorId}/schedules")]
    public async Task<IActionResult> GetDoctorSchedules(string doctorId, CancellationToken cancellationToken)
    {
        var schedules = await _bookingService.GetDoctorSchedulesAsync(doctorId, cancellationToken);

        return Ok(schedules);
    }

    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetBookingScreenDetails(string doctorId, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetBookingScreenDetailsAsync(doctorId, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
    }
}
