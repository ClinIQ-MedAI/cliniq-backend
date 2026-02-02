using Microsoft.AspNetCore.Authorization;
using Clinic.Authentication.Authorization;
using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Profile.Patient.Controllers;

/// <summary>
/// Patient survey controller.
/// Registration, verification, and login are handled by the shared /api/auth endpoints.
/// This controller handles patient survey submission only.
/// </summary>
[Route("patient/[controller]")]
[ApiController]
public class SurveyController(
    PatientRegistrationService registrationService
    ) : ControllerBase
{
    private readonly PatientRegistrationService _registrationService = registrationService;

    /// <summary>
    /// Submit patient survey to create PatientProfile.
    /// Requires authenticated and verified user.
    /// Creates PatientProfile with ACTIVE status.
    /// </summary>
    [HttpPost("")]
    [Authorize(Policy = PolicyNames.VerifiedUser)]
    public async Task<IActionResult> SubmitSurvey([FromBody] PatientSurveyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _registrationService.SubmitSurveyAsync(userId, request, cancellationToken);

        if (result.IsSucceed)
            return Ok(new { Message = "Patient profile created successfully." });

        return result.ToProblem();
    }
}
