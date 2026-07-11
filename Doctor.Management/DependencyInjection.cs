using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Doctor.Management.Services;
using Mapster;

namespace Doctor.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddDoctorManagementModule(
        this IServiceCollection services)
    {
        services.AddScoped<IDoctorManagementService, DoctorManagementService>();

        // Register mapping if any (currently manual or using Adapt)

        return services;
    }
}
