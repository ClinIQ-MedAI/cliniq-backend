using Microsoft.Extensions.DependencyInjection;

namespace Booking.Management;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingManagementModule(this IServiceCollection services)
    {
        // Register services here
        return services;
    }
}
