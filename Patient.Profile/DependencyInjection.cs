using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patient.Profile.Mapping;

namespace Patient.Profile;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Patient Profile module services.
    /// </summary>
    public static IServiceCollection AddPatientProfileModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Patient-specific services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<PatientRegistrationService>();

        // Register mapping configurations
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(typeof(MappingConfigurations).Assembly);

        return services;
    }
}
