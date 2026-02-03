using Clinic.Infrastructure;
using Clinic.Authentication;
using Patient.Profile;
using Doctor.Profile;
using Patient.Management;
using Doctor.Management;
using Booking.Management;
using Booking.Doctor;
using Booking.Patient;
using Serilog;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Clinic.Authentication.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(Patient.Profile.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Doctor.Profile.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Patient.Management.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Doctor.Management.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Booking.Management.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Booking.Doctor.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Booking.Patient.DependencyInjection).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Clinic API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication Module
builder.Services.AddAuthenticationModule(builder.Configuration);

// Modules
builder.Services.AddPatientProfileModule(builder.Configuration);
builder.Services.AddDoctorProfileModule(builder.Configuration);
builder.Services.AddPatientManagementModule();
builder.Services.AddDoctorManagementModule();
builder.Services.AddBookingManagementModule();
builder.Services.AddBookingDoctorModule();
builder.Services.AddBookingPatientModule();

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
        options.Title = "Clinic API";
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

app.MapGroup("api").MapControllers();

app.Run();
