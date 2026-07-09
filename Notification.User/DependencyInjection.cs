using Microsoft.Extensions.DependencyInjection;
using Notification.User.Services;

namespace Notification.User;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationUserModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
