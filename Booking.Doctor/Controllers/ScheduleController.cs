using Booking.Doctor.Contracts;
using Booking.Doctor.Services;
using Clinic.Authentication.Authorization;

namespace Booking.Doctor.Controllers;

[ApiController]
[Route("schedules")]
[Authorize(Policy = PolicyNames.ActiveDoctor)]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpPost("availability")]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var doctorId = User.GetUserId();

        var result = await _scheduleService.SetAvailabilityAsync(doctorId!, request, cancellationToken);

        return result.IsSucceed ? Ok() : result.ToProblem();
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateSchedules(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var doctorId = User.GetUserId();

        var result = await _scheduleService.GenerateSchedulesAsync(doctorId!, startDate, endDate, cancellationToken);

        return result.IsSucceed ? Ok() : result.ToProblem();
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var doctorId = User.GetUserId();

        var result = await _scheduleService.GetSchedulesAsync(doctorId!, from, to, cancellationToken);

        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }
}
