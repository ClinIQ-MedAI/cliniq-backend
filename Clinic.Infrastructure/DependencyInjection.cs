using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Clinic.Infrastructure.Services;
using Clinic.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Clinic.Infrastructure.Health;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Clinic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlServerOptions =>
            {
                sqlServerOptions.CommandTimeout(180); // Set timeout to 3 minutes
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
            }));

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

        // Redis Connection Multiplexer
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ConnectionMultiplexer>>();
            var connectionString = configuration.GetConnectionString("Redis");
            
            ConnectionMultiplexer multiplexer;
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogWarning("Redis connection string is missing or empty. Defaulting to localhost:6379");
                multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
            }
            else
            {
                var configurationOptions = ConfigurationOptions.Parse(connectionString);
                logger.LogInformation("Establishing connection to Redis endpoint(s)...");
                multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                logger.LogInformation("Successfully connected to Redis.");
            }

            // Hook up events for connection logging
            multiplexer.ConnectionFailed += (sender, e) =>
            {
                logger.LogError(e.Exception, "Redis connection failed. Endpoint: {Endpoint}, FailureType: {FailureType}", e.EndPoint, e.FailureType);
            };

            multiplexer.ConnectionRestored += (sender, e) =>
            {
                logger.LogInformation("Redis connection restored. Endpoint: {Endpoint}", e.EndPoint);
            };

            multiplexer.InternalError += (sender, e) =>
            {
                logger.LogError(e.Exception, "Redis internal error. Endpoint: {Endpoint}, Origin: {Origin}", e.EndPoint, e.Origin);
            };

            return multiplexer;
        });

        // Redis Cache using the shared ConnectionMultiplexer
        services.AddStackExchangeRedisCache(options => { });
        services.AddOptions<RedisCacheOptions>()
            .Configure<IServiceProvider>((options, sp) =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>());
            });

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<MailProviderHealthCheck>("mail_provider")
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection") ?? string.Empty, name: "sql_server")
            .AddCheck<RedisHealthCheck>("redis");

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
