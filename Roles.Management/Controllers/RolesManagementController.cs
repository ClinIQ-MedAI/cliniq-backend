using System.Reflection;
using Roles.Management.Services;

namespace Roles.Management.Controllers;

[Route("admin/roles")]
[ApiController]
public class RolesManagementController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet("")]
    [HasPermission(Permissions.ViewRoles)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.ViewRoles)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.ViewRoles)]
    public IActionResult GetPermissions()
    {
        var permissions = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToArray();

        return Ok(permissions);
    }

    [HttpPost("")]
    [HasPermission(Permissions.CreateRoles)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateAsync(request);
        return result.IsSucceed
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateRoles)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _roleService.UpdateAsync(id, request);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteRoles)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var result = await _roleService.DeleteAsync(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
