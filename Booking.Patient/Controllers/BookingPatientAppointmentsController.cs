using Booking.Patient.Contracts;
using Booking.Patient.Services;
using Clinic.Authentication.Authorization;

namespace Booking.Patient.Controllers;

[ApiController]
[Route("patient")]
public class BookingPatientAppointmentsController : ControllerBase
{
    private readonly IBookingPatientService _bookingService;

    public BookingPatientAppointmentsController(IBookingPatientService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("appointments")]
    [Authorize(Policy = PolicyNames.ActivePatient)]
    public async Task<IActionResult> GetExaminationAppointments(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var appointments = await _bookingService.GetExaminationAppointmentsAsync(userId!, cancellationToken);

        return Ok(new ApiResponse<List<FlutterAppointmentDto>>(true, "Appointments fetched successfully", appointments));
    }

    [HttpGet("doctors/available")]
    public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var doctors = await _bookingService.GetAvailableDoctorsFlutterAsync(date, cancellationToken);

        return Ok(new ApiResponse<List<FlutterAppointmentDto>>(true, "Doctors fetched successfully", doctors));
    }

    [HttpGet("doctors/{doctorId}/schedule")]
    public async Task<IActionResult> GetDoctorSchedule(string doctorId, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetDoctorScheduleFlutterAsync(doctorId, cancellationToken);

        if (result.IsFailure)
            return NotFound(new ApiResponse<object>(false, result.Error.Description, null));

        return Ok(new ApiResponse<FlutterDoctorScheduleDataDto>(true, "Schedule fetched successfully", result.Value));
    }

    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetDoctorDetails(string doctorId, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetDoctorDetailsFlutterAsync(doctorId, cancellationToken);

        if (result.IsFailure)
            return NotFound(new ApiResponse<object>(false, result.Error.Description, null));

        return Ok(new ApiResponse<FlutterDoctorDetailsDataDto>(true, "Doctor details fetched successfully", result.Value));
    }

    [HttpPost("appointments")]
    [Authorize(Policy = PolicyNames.ActivePatient)]
    public async Task<IActionResult> BookAppointment([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _bookingService.BookAppointmentAsync(userId!, request.DoctorId, request.Date, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.StatusCode switch
            {
                404 => NotFound(new ApiResponse<object>(false, result.Error.Description, null)),
                409 => Conflict(new ApiResponse<object>(false, result.Error.Description, null)),
                _ => BadRequest(new ApiResponse<object>(false, result.Error.Description, null))
            };
        }

        return Ok(new ApiResponse<object>(true, "Appointment booked successfully", null));
    }
}
