using Contact.Management.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Contact.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddContactManagementModule(this IServiceCollection services)
    {
        services.AddScoped<IContactManagementService, ContactManagementService>();
        return services;
    }
}
