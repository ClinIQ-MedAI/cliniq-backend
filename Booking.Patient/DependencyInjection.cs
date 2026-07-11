using Booking.Patient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Patient;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingPatientModule(this IServiceCollection services)
    {
        services.AddScoped<IBookingPatientService, BookingPatientService>();
        services.AddScoped<IBookingPatientHomeService, BookingPatientHomeService>();
        return services;
    }
}
