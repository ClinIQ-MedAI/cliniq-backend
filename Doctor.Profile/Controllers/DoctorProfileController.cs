using Clinic.Infrastructure.Contracts.Doctors;

namespace Doctor.Profile.Controllers;

[Route("doctor/me")]
[ApiController]
[Authorize(Policy = PolicyNames.ActiveDoctor)]
public class DoctorProfileController(IDoctorUserService userService) : ControllerBase
{
    private readonly IDoctorUserService _userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var userId = User.GetUserId()!;

        var result = await _userService.GetProfileAsync(userId);

        return Ok(result.Value);
    }

    [HttpPut("")]
    public async Task<IActionResult> Info([FromBody] UpdateProfileRequest request)
    {
        var userId = User.GetUserId()!;

        await _userService.UpdateProfileAsync(userId, request);

        return NoContent();
    }

    [HttpPut("update-request")]
    public async Task<IActionResult> SubmitUpdateRequest([FromBody] SubmitDoctorUpdateRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;

        var result = await _userService.SubmitUpdateRequestAsync(userId, request, cancellationToken);

        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.GetUserId()!;

        var result = await _userService.ChangePasswordAsync(userId, request);

        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
