using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services;

namespace Clinic.Authentication.Authorization;

public class PermissionService(
    ICacheService cacheService,
    RoleManager<ApplicationRole> roleManager) : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
    {
        var roles = GetRoles(user);
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var permissions = await GetCachedPermissionsAsync(role);
            if (permissions.Contains(permission))
                return true;
        }

        return false;
    }

    public async Task<string[]> GetPermissionsForUserAsync(ClaimsPrincipal user)
    {
        var roles = GetRoles(user);
        var allPermissions = new HashSet<string>();

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var permissions = await GetCachedPermissionsAsync(role);
            allPermissions.UnionWith(permissions);
        }

        return [.. allPermissions];
    }

    public async Task InvalidateRoleCacheAsync(string roleId)
    {
        await cacheService.RemoveAsync($"role:{roleId}:permissions");
    }

    private static List<string> GetRoles(ClaimsPrincipal user)
    {
        return user.Claims
            .Where(c => c.Type == "roles")
            .Select(c => c.Value)
            .ToList();
    }

    private async Task<string[]> GetCachedPermissionsAsync(ApplicationRole role)
    {
        var cacheKey = $"role:{role.Id}:permissions";
        var permissions = await cacheService.GetAsync<string[]>(cacheKey);

        if (permissions is not null)
            return permissions;

        permissions = (await roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToArray();

        await cacheService.SetAsync(cacheKey, permissions, CacheTtl);

        return permissions;
    }
}
