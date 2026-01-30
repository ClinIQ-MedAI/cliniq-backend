using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DoctorAPI.Services;
using DoctorAPI.Mapping;
using Mapster;

namespace DoctorAPI;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Doctor module services.
    /// </summary>
    public static IServiceCollection AddDoctorModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Doctor-specific services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<DoctorRegistrationService>();

        // Register mapping configurations
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(typeof(MappingConfigurations).Assembly);

        // Note: Authentication validators are registered via Clinic.Authentication module

        return services;
    }
}
