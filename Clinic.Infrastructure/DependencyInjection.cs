using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Clinic.Infrastructure.Services;
using Clinic.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Clinic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Add HttpContextAccessor if not already added
        services.AddHttpContextAccessor();

        // Note: Authentication services are now registered via Clinic.Authentication.AddAuthenticationModule()
        // Infrastructure only provides the core database and identity setup

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Application Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IOtpService, OtpService>();

        // Email Configuration
        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
        services.AddTransient<IEmailSender, EmailService>();

        services.AddScoped<INotificationService, NotificationService>();

        services.AddLocalization();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        return app;
    }
}
