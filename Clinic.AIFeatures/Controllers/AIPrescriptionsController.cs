using Clinic.AIFeatures.Contracts;
using Clinic.AIFeatures.Services;
using Clinic.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.AIFeatures.Controllers;

[Authorize]
[ApiController]
[Route("ai/prescriptions")]
public class AIPrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public AIPrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadPrescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _prescriptionService.UploadPrescriptionAsync(request, cancellationToken);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var result = await _prescriptionService.GetPrescriptionAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetPatientPrescriptions([FromRoute] string patientId)
    {
        var result = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);
        return Ok(result);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> Confirm([FromRoute] int id, [FromBody] ConfirmPrescriptionRequest request)
    {
        var result = await _prescriptionService.ConfirmPrescriptionAsync(id, request);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
