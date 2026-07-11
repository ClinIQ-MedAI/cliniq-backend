using Contact.Public.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Contact.Public;

public static class DependencyInjection
{
    public static IServiceCollection AddContactPublicModule(this IServiceCollection services)
    {
        services.AddScoped<IContactPublicService, ContactPublicService>();
        return services;
    }
}
