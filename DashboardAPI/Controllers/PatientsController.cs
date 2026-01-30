using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Infrastructure.Authentication;
using DashboardAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    private readonly IPatientService _patientService = patientService;

    [HttpGet("")]
    [HasPermission(Permissions.GetPatients)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _patientService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetPatients)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _patientService.GetAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddPatients)]
    public async Task<IActionResult> Add([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var result = await _patientService.AddAsync(request, cancellationToken);
        return result.IsSucceed ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdatePatients)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var result = await _patientService.UpdateAsync(id, request, cancellationToken);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPut("{id}/toggle-status")]
    [HasPermission(Permissions.UpdatePatients)]
    public async Task<IActionResult> ToggleStatus([FromRoute] string id)
    {
        var result = await _patientService.ToggleStatus(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPut("{id}/unlock")]
    [HasPermission(Permissions.UpdatePatients)]
    public async Task<IActionResult> Unlock([FromRoute] string id)
    {
        var result = await _patientService.Unlock(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}


