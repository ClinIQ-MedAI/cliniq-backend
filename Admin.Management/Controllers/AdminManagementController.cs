using Admin.Management.Services;

namespace Admin.Management.Controllers;

[Route("admin/admins")]
[ApiController]
public class AdminManagementController(IAdminManagementService adminService) : ControllerBase
{
    private readonly IAdminManagementService _adminService = adminService;

    [HttpGet("")]
    [HasPermission(Permissions.GetAdmins)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllAsync(cancellationToken);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetAdmins)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _adminService.GetByIdAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.AddAdmins)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateAsync(request, cancellationToken);
        return result.IsSucceed
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpPatch("{id}/status")]
    [HasPermission(Permissions.UpdateAdmins)]
    public async Task<IActionResult> ToggleDisable([FromRoute] string id)
    {
        var result = await _adminService.ToggleDisableAsync(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteAdmins)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var result = await _adminService.DeleteAsync(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
