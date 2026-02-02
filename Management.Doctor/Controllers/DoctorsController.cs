using Clinic.Infrastructure.Contracts.Doctors;
using Management.Doctor.Services;

namespace Management.Doctor.Controllers;

[Route("admin/[controller]")]
[ApiController]
public class DoctorsController(IDoctorService doctorService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;

    [HttpGet("")]
    [HasPermission(Permissions.GetDoctors)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _doctorService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetDoctors)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _doctorService.GetAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddDoctors)]
    public async Task<IActionResult> Add([FromBody] CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorService.AddAsync(request, cancellationToken);
        return result.IsSucceed ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateDoctors)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorService.UpdateAsync(id, request, cancellationToken);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPatch("{id}/status")]
    [HasPermission(Permissions.UpdateDoctors)]
    public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] UpdateProfileStatusRequest request)
    {
        var result = await _doctorService.UpdateStatusAsync(id, request.Active);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPut("{id}/unlock")]
    [HasPermission(Permissions.UpdateDoctors)]
    public async Task<IActionResult> Unlock([FromRoute] string id)
    {
        var result = await _doctorService.Unlock(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id}/approve")]
    [HasPermission(Permissions.UpdateDoctors)]
    public async Task<IActionResult> Approve([FromRoute] string id)
    {
        var result = await _doctorService.ApproveAsync(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPost("{id}/reject")]
    [HasPermission(Permissions.UpdateDoctors)]
    public async Task<IActionResult> Reject([FromRoute] string id, [FromBody] RejectDoctorRequest request)
    {
        var result = await _doctorService.RejectAsync(id, request.Reason);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
