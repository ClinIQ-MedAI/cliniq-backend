using Clinic.Infrastructure.Contracts.Doctors;
using Doctor.Profile.Localization;
using Microsoft.Extensions.Localization;

namespace Doctor.Profile.Controllers;

/// <summary>
/// Doctor survey controller.
/// Registration, verification, and login are handled by the shared /api/auth endpoints.
/// This controller handles doctor survey submission only.
/// </summary>
[Route("doctor/[controller]")]
[ApiController]
public class SurveyController(
    DoctorRegistrationService registrationService,
    IStringLocalizer<Messages> localizer
    ) : ControllerBase
{
    private readonly DoctorRegistrationService _registrationService = registrationService;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    /// <summary>
    /// Submit doctor survey to create DoctorProfile.
    /// Requires authenticated and verified user.
    /// Creates DoctorProfile with PENDING_VERIFICATION status.
    /// </summary>
    [HttpPost("")]
    [Authorize(Policy = PolicyNames.VerifiedUser)]
    public async Task<IActionResult> SubmitSurvey([FromBody] DoctorSurveyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;

        var result = await _registrationService.SubmitSurveyAsync(userId, request, cancellationToken);

        return result.IsSucceed ?
            Ok(new { Message = _localizer["SurveyCreated"].Value }) :
            result.ToProblem();
    }
}
