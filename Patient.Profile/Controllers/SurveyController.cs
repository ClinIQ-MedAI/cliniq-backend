using Clinic.Infrastructure.Contracts.Patients;
using Patient.Profile.Localization;
using Microsoft.Extensions.Localization;

namespace Patient.Profile.Controllers;

/// <summary>
/// Patient survey controller.
/// Registration, verification, and login are handled by the shared /api/auth endpoints.
/// This controller handles patient survey submission only.
/// </summary>
[Route("patient/[controller]")]
[ApiController]
public class SurveyController(
    PatientRegistrationService registrationService,
    IStringLocalizer<Messages> localizer
    ) : ControllerBase
{
    private readonly PatientRegistrationService _registrationService = registrationService;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    /// <summary>
    /// Submit patient survey to create PatientProfile.
    /// Requires authenticated and verified user.
    /// Creates PatientProfile with ACTIVE status.
    /// </summary>
    [HttpPost("")]
    [Authorize(Policy = PolicyNames.VerifiedUser)]
    public async Task<IActionResult> SubmitSurvey([FromBody] PatientSurveyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;

        var result = await _registrationService.SubmitSurveyAsync(userId, request, cancellationToken);

        return result.IsSucceed ?
            Ok(new { Message = _localizer["SurveyCreated"].Value }) :
            result.ToProblem();
    }
}
