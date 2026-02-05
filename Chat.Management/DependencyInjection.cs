using Chat.Management.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddChatManagementModule(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        return services;
    }
}
