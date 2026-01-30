using Clinic.Infrastructure;
using Clinic.Authentication;
using PatientAPI;
using DoctorAPI;
using DashboardAPI;
using Serilog;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Clinic.Authentication.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(PatientAPI.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(DoctorAPI.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(DashboardAPI.DependencyInjection).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "ClinicAPI Main Gateway";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication Module
builder.Services.AddAuthenticationModule(builder.Configuration);

// Modules
builder.Services.AddPatientModule(builder.Configuration);
builder.Services.AddDoctorModule(builder.Configuration);
builder.Services.AddDashboardModule(builder.Configuration);

// Logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ClinicAPI Gateway";
        options.Theme = ScalarTheme.Mars;
        options.ShowSidebar = true;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.OpenApiRoutePattern = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseInfrastructure(); // Custom pipeline from Infrastructure

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
