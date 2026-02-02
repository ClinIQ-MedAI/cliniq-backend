using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Management.Patient.Services;
using Mapster;

namespace Management.Patient;

public static class DependencyInjection
{
    public static IServiceCollection AddPatientManagementModule(
        this IServiceCollection services)
    {
        services.AddScoped<IPatientService, PatientService>();
        
        return services;
    }
}
