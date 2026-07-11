using Chat.Doctor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Doctor;

public static class DependencyInjection
{
    public static IServiceCollection AddChatDoctorModule(this IServiceCollection services)
    {
        services.AddScoped<IChatDoctorService, ChatDoctorService>();
        return services;
    }
}
