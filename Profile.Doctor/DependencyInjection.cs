using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profile.Doctor.Services;
using Profile.Doctor.Mapping;
using Mapster;

namespace Profile.Doctor;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Doctor Profile module services.
    /// </summary>
    public static IServiceCollection AddDoctorProfileModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Doctor-specific services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<DoctorRegistrationService>();

        // Register mapping configurations
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(typeof(MappingConfigurations).Assembly);

        return services;
    }
}
