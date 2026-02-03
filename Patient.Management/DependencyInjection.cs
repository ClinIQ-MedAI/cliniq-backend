using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patient.Management.Services;
using Mapster;

namespace Patient.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddPatientManagementModule(
        this IServiceCollection services)
    {
        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}
