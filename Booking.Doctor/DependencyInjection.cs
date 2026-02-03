using Booking.Doctor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Doctor;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingDoctorModule(this IServiceCollection services)
    {
        services.AddScoped<IScheduleService, ScheduleService>();
        return services;
    }
}
