namespace Clinic.Authentication.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public class PermissionHandler(
    IAuthPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAuthPermissionService _permissionService = permissionService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await _permissionService.HasPermissionAsync(context.User, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
