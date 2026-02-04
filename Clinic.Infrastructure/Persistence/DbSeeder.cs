using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager, context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = ["SuperAdmin", "Admin"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        // 1. Super Admin
        await CreateUserAsync(userManager, "Super Admin", "superadmin@clinic.com", "Password123!", "SuperAdmin");
        Console.WriteLine("Super Admin seeded successfully.");

        // 2. Admin
        await CreateUserAsync(userManager, "Admin User", "admin@clinic.com", "Password123!", "Admin");
        Console.WriteLine("Admin seeded successfully.");

        // 3. Active Doctor
        var doctorUser = await CreateUserAsync(userManager, "Dr. John Doe", "doctor@clinic.com", "Password123!");
        if (doctorUser != null)
        {
            await EnsureDoctorProfileAsync(context, doctorUser.Id);
        }
        Console.WriteLine("Doctor seeded successfully.");

        // 4. Active Patient
        var patientUser = await CreateUserAsync(userManager, "Jane Doe", "patient@clinic.com", "Password123!");
        if (patientUser != null)
        {
            await EnsurePatientProfileAsync(context, patientUser.Id);
        }
        Console.WriteLine("Patient seeded successfully.");
    }

    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string name,
        string email,
        string password,
        string? role = null)
    {
        if (await userManager.FindByEmailAsync(email) != null)
        {
            return await userManager.FindByEmailAsync(email);
        }

        var names = name.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : string.Empty;

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            PhoneNumber = "1234567890", // Dummy phone
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded && role is not null)
        {
            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        return null;
    }

    private static async Task EnsureDoctorProfileAsync(AppDbContext context, string userId)
    {
        if (!await context.DoctorProfiles.AnyAsync(d => d.Id == userId))
        {
            context.DoctorProfiles.Add(new DoctorProfile
            {
                Id = userId,
                Status = DoctorStatus.ACTIVE,
                Specialization = "Cardiology",
                LicenseNumber = "LIC-12345",
                LicenseExpiryDate = DateTime.UtcNow.AddYears(1),
                MedicalLicenseUrl = "https://example.com/license.pdf",
                PersonalIdentityPhotoUrl = "https://example.com/photo.jpg"
            });

            // Seed Availability (Mon, Wed, Fri from 9 AM to 5 PM)
            var days = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            foreach (var day in days)
            {
                context.DoctorAvailabilities.Add(new DoctorAvailability
                {
                    DoctorId = userId,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    MaxBookings = 10,
                    IsAvailable = true
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsurePatientProfileAsync(AppDbContext context, string userId)
    {
        if (!await context.PatientProfiles.AnyAsync(p => p.Id == userId))
        {
            context.PatientProfiles.Add(new PatientProfile
            {
                Id = userId,
                Status = PatientStatus.ACTIVE,
                BloodType = "O+",
                Height = 175,
                Weight = 70,
                HasDiabetes = false,
                HasPressureIssues = false,
                EmergencyContactName = "Emergency Contact",
                EmergencyContactPhone = "9876543210"
            });
            await context.SaveChangesAsync();
        }
    }
}
