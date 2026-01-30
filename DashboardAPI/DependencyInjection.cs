using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DashboardAPI;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Dashboard module services.
    /// </summary>
    public static IServiceCollection AddDashboardModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Dashboard-specific services
        // Register Dashboard-specific services
        services.AddScoped<Services.IPatientService, Services.PatientService>();
        services.AddScoped<Services.IDoctorService, Services.DoctorService>();

        return services;
    }
}
