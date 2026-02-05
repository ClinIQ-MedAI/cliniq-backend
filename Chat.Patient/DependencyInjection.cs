using Chat.Patient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Patient;

public static class DependencyInjection
{
    public static IServiceCollection AddChatPatientModule(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        return services;
    }
}
