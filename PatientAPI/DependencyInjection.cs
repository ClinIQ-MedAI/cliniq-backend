using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientAPI.Services;
using PatientAPI.Mapping;
using Mapster;

namespace PatientAPI;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Patient module services.
    /// </summary>
    public static IServiceCollection AddPatientModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Patient-specific services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<PatientRegistrationService>();

        // Register mapping configurations
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(typeof(MappingConfigurations).Assembly);

        // Note: Authentication validators are registered via Clinic.Authentication module

        return services;
    }
}
