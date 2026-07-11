using Roles.Management.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Roles.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddRolesManagementModule(
        this IServiceCollection services)
    {
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
