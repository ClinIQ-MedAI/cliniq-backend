using Clinic.Authentication.Authorization;
using Clinic.Authentication.Jwt;
using Clinic.Authentication.Services;
using Clinic.Authentication.Strategies;
using Clinic.Infrastructure.Entities.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Authentication;

/// <summary>
/// Dependency injection extensions for Clinic.Authentication module.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the authentication module services.
    /// </summary>
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT options
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Add JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add authorization policies
        services.AddAuthorization(options =>
        {
            // Verified user - requires email OR phone verified
            options.AddPolicy(PolicyNames.VerifiedUser, policy =>
                policy.AddRequirements(new VerificationRequirement(requireAny: true)));

            // Active patient
            options.AddPolicy(PolicyNames.ActivePatient, policy =>
                policy.AddRequirements(new PatientStatusRequirement(PatientStatus.ACTIVE.ToString())));

            // Active doctor
            options.AddPolicy(PolicyNames.ActiveDoctor, policy =>
                policy.AddRequirements(new DoctorStatusRequirement(DoctorStatus.ACTIVE.ToString())));

            // Patient profile required (not incomplete)
            options.AddPolicy(PolicyNames.PatientProfileRequired, policy =>
                policy.AddRequirements(new PatientStatusRequirement(
                    PatientStatus.ACTIVE.ToString(),
                    PatientStatus.SUSPENDED.ToString())));

            // Doctor profile required (not incomplete)
            options.AddPolicy(PolicyNames.DoctorProfileRequired, policy =>
                policy.AddRequirements(new DoctorStatusRequirement(
                    DoctorStatus.PENDING_VERIFICATION.ToString(),
                    DoctorStatus.REJECTED.ToString(),
                    DoctorStatus.ACTIVE.ToString(),
                    DoctorStatus.SUSPENDED.ToString())));

            // Pending doctor
            options.AddPolicy(PolicyNames.PendingDoctor, policy =>
                policy.AddRequirements(new DoctorStatusRequirement(DoctorStatus.PENDING_VERIFICATION.ToString())));
        });

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, VerificationRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, PatientStatusRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, DoctorStatusRequirementHandler>();

        // Register JWT provider
        services.AddScoped<IJwtProvider, JwtProvider>();

        // Register login strategies
        services.AddScoped<ILoginStrategy, PasswordLoginStrategy>();
        services.AddScoped<ILoginStrategy, OtpLoginStrategy>();

        // Register services
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IVerificationService, VerificationService>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
