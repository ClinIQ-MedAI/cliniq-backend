using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Management.Doctor.Services;
using Mapster;

namespace Management.Doctor;

public static class DependencyInjection
{
    public static IServiceCollection AddDoctorManagementModule(
        this IServiceCollection services)
    {
        services.AddScoped<IDoctorService, DoctorService>();
        
        // Register mapping if any (currently manual or using Adapt)
        
        return services;
    }
}
