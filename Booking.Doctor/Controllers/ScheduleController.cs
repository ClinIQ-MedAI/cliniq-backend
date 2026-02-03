using Booking.Doctor.Contracts;
using Booking.Doctor.Services;
using Clinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Doctor.Controllers;

[ApiController]
[Route("schedules")]
[Authorize]
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
        if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

        await _scheduleService.SetAvailabilityAsync(doctorId, request, cancellationToken);
        return Ok();
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateSchedules(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var doctorId = User.GetUserId();
        if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

        await _scheduleService.GenerateSchedulesAsync(doctorId, startDate, endDate, cancellationToken);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var doctorId = User.GetUserId();
        if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

        var schedules = await _scheduleService.GetSchedulesAsync(doctorId, from, to, cancellationToken);
        return Ok(schedules);
    }
}
