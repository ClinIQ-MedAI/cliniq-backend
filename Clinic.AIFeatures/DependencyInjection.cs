using Clinic.AIFeatures.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.AIFeatures;

public static class DependencyInjection
{
    public static IServiceCollection AddAIFeaturesModule(this IServiceCollection services)
    {
        services.AddScoped<IScanService, ScanService>();
        services.AddScoped<IChatbotService, ChatbotService>();
        return services;
    }
}
