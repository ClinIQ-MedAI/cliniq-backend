using Clinic.Authentication.Authorization;
using Clinic.Authentication.Contracts.Roles;
using Clinic.Authentication.Services;
using Clinic.Infrastructure.Extensions;

namespace Clinic.Authentication.Controllers;

[Route("admin/[controller]")]
[ApiController]
[Authorize(Policy = PolicyNames.Admin)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateAsync(request);
        return result.IsSucceed
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _roleService.UpdateAsync(id, request);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var result = await _roleService.DeleteAsync(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
