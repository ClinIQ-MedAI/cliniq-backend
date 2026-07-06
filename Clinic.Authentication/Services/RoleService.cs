using System.Text.Json;
using Clinic.Authentication.Authorization;
using Clinic.Authentication.Contracts.Roles;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;

namespace Clinic.Authentication.Services;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    IPermissionService permissionService) : IRoleService
{
    public async Task<Result<List<RoleResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var response = roles.Select(MapToResponse).ToList();
        return Result.Succeed(response);
    }

    public async Task<Result<RoleResponse>> GetByIdAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null || role.IsDeleted)
            return Result.Failure<RoleResponse>(Error.NotFound("Role.NotFound", "Role not found"));

        return Result.Succeed(MapToResponse(role));
    }

    public async Task<Result<RoleResponse>> CreateAsync(CreateRoleRequest request)
    {
        if (await roleManager.RoleExistsAsync(request.Name))
            return Result.Failure<RoleResponse>(Error.Conflict("Role.AlreadyExists", "Role already exists"));

        var role = new ApplicationRole
        {
            Name = request.Name,
            Permissions = JsonSerializer.Serialize(request.Permissions ?? [])
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return Result.Failure<RoleResponse>(Error.BadRequest("Role.CreateFailed",
                string.Join(", ", result.Errors.Select(e => e.Description))));

        return Result.Succeed(MapToResponse(role));
    }

    public async Task<Result<RoleResponse>> UpdateAsync(string id, UpdateRoleRequest request)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null || role.IsDeleted)
            return Result.Failure<RoleResponse>(Error.NotFound("Role.NotFound", "Role not found"));

        if (role.IsDefault)
            return Result.Failure<RoleResponse>(Error.BadRequest("Role.CannotEditDefault", "Cannot edit a default role"));

        if (request.Name is not null)
        {
            role.Name = request.Name;
        }

        if (request.Permissions is not null)
        {
            role.Permissions = JsonSerializer.Serialize(request.Permissions);
            await permissionService.InvalidateRoleCacheAsync(role.Id!);
        }

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return Result.Failure<RoleResponse>(Error.BadRequest("Role.UpdateFailed",
                string.Join(", ", result.Errors.Select(e => e.Description))));

        return Result.Succeed(MapToResponse(role));
    }

    public async Task<Result> DeleteAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null || role.IsDeleted)
            return Result.Failure(Error.NotFound("Role.NotFound", "Role not found"));

        if (role.IsDefault)
            return Result.Failure(Error.BadRequest("Role.CannotDeleteDefault", "Cannot delete a default role"));

        role.IsDeleted = true;
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return Result.Failure(Error.BadRequest("Role.DeleteFailed",
                string.Join(", ", result.Errors.Select(e => e.Description))));

        await permissionService.InvalidateRoleCacheAsync(role.Id!);
        return Result.Succeed();
    }

    private static RoleResponse MapToResponse(ApplicationRole role) => new(
        role.Id!,
        role.Name!,
        role.IsDefault,
        role.IsDeleted,
        DeserializePermissions(role.Permissions)
    );

    private static string[] DeserializePermissions(string? permissionsJson)
    {
        if (string.IsNullOrWhiteSpace(permissionsJson)) return [];
        try { return JsonSerializer.Deserialize<string[]>(permissionsJson) ?? []; }
        catch { return []; }
    }
}
