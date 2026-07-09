using Microsoft.Extensions.DependencyInjection;
using Notification.Management.Services;

namespace Notification.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationManagementModule(this IServiceCollection services)
    {
        services.AddScoped<Services.INotificationService, NotificationService>();
        return services;
    }
}
