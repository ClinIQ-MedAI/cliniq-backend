using Clinic.Authentication.Contracts.Users;

namespace Profile.Patient.Controllers;

[Route("patient/me")]
[ApiController]
[Authorize]
public class ProfileController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

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

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.GetUserId()!;

        var result = await _userService.ChangePasswordAsync(userId, request);

        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
