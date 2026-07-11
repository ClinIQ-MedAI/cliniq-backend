using Clinic.AIFeatures.Contracts;
using Clinic.AIFeatures.Services;
using Clinic.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.AIFeatures.Controllers;

[Authorize]
[ApiController]
[Route("ai/scans")]
public class AIScansController : ControllerBase
{
    private readonly IScanService _scanService;

    public AIScansController(IScanService scanService)
    {
        _scanService = scanService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadScanRequest request, CancellationToken cancellationToken)
    {
        var result = await _scanService.UploadScanAsync(request, cancellationToken);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var result = await _scanService.GetScanAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetPatientScans([FromRoute] string patientId)
    {
        var result = await _scanService.GetPatientScansAsync(patientId);
        return Ok(result);
    }

    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> Review([FromRoute] int id, [FromBody] ReviewScanRequest request)
    {
        var result = await _scanService.ReviewScanAsync(id, request);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
