namespace Clinic.Authentication.Authorization;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);
    Task<string[]> GetPermissionsForUserAsync(ClaimsPrincipal user);
    Task InvalidateRoleCacheAsync(string roleId);
}
