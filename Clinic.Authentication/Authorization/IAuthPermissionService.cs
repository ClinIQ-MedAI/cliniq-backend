namespace Clinic.Authentication.Authorization;

public interface IAuthPermissionService
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);
    Task<string[]> GetPermissionsForUserAsync(ClaimsPrincipal user);
    Task InvalidateRoleCacheAsync(string roleId);
}
