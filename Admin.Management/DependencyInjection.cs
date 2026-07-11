using Admin.Management.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminManagementModule(
        this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
