using Clinic.Infrastructure.Entities;

namespace Roles.Management.Services;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    IPermissionService permissionService) : IRoleService
{
    public async Task<Result<List<RoleResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var response = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var permissions = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToArray();
            response.Add(MapToResponse(role, permissions));
        }

        return Result.Succeed(response);
    }

    public async Task<Result<RoleResponse>> GetByIdAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null || role.IsDeleted)
            return Result.Failure<RoleResponse>(Error.NotFound("Role.NotFound", "Role not found"));

        var permissions = (await roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToArray();

        return Result.Succeed(MapToResponse(role, permissions));
    }

    public async Task<Result<RoleResponse>> CreateAsync(CreateRoleRequest request)
    {
        if (await roleManager.RoleExistsAsync(request.Name))
            return Result.Failure<RoleResponse>(Error.Conflict("Role.AlreadyExists", "Role already exists"));

        var role = new ApplicationRole { Name = request.Name };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return Result.Failure<RoleResponse>(Error.BadRequest("Role.CreateFailed",
                string.Join(", ", result.Errors.Select(e => e.Description))));

        foreach (var permission in request.Permissions ?? [])
            await roleManager.AddClaimAsync(role, new Claim("permission", permission));

        var permissions = request.Permissions ?? [];
        return Result.Succeed(MapToResponse(role, permissions));
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
            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in existingClaims.Where(c => c.Type == "permission"))
                await roleManager.RemoveClaimAsync(role, claim);

            foreach (var permission in request.Permissions)
                await roleManager.AddClaimAsync(role, new Claim("permission", permission));

            await permissionService.InvalidateRoleCacheAsync(role.Id!);
        }

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return Result.Failure<RoleResponse>(Error.BadRequest("Role.UpdateFailed",
                string.Join(", ", result.Errors.Select(e => e.Description))));

        var permissions = request.Permissions ?? (await roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToArray();

        return Result.Succeed(MapToResponse(role, permissions));
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

    private static RoleResponse MapToResponse(ApplicationRole role, string[] permissions) => new(
        role.Id!,
        role.Name!,
        role.IsDefault,
        role.IsDeleted,
        permissions
    );
}
