using System.Security.Claims;
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

        // Ensure database is created and migrations are applied
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager, context);
        await SeedUsersAsync(userManager, context);
        await SeedNewsAsync(context);
        await SeedDashboardDataAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, AppDbContext context)
    {
        // Fix any null concurrency stamps in AspNetRoles and AspNetUsers
        // This prevents DbUpdateConcurrencyException in SQL Server when updating rows with null ConcurrencyStamps
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE AspNetRoles SET ConcurrencyStamp = NEWID() WHERE ConcurrencyStamp IS NULL");
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE AspNetUsers SET ConcurrencyStamp = NEWID() WHERE ConcurrencyStamp IS NULL");

        context.ChangeTracker.Clear(); // Clear the tracker to ensure we work with clean state after raw SQL updates

        var superAdminPermissions = new[]
        {
            "Permissions.Patients.View",
            "Permissions.Patients.Create",
            "Permissions.Patients.Update",
            "Permissions.Patients.Delete",
            "Permissions.Doctors.View",
            "Permissions.Doctors.Create",
            "Permissions.Doctors.Update",
            "Permissions.Doctors.Delete",
            "Permissions.Roles.View",
            "Permissions.Roles.Create",
            "Permissions.Roles.Update",
            "Permissions.Roles.Delete",
            "Permissions.Bookings.View",
            "Permissions.Bookings.Update",
            "Permissions.Chats.View",
            "Permissions.Notifications.Send",
            "Permissions.Dashboard.View",
            "Permissions.Contacts.Manage",
        };

        var adminPermissions = new[]
        {
            "Permissions.Patients.View",
            "Permissions.Patients.Create",
            "Permissions.Patients.Update",
            "Permissions.Doctors.View",
            "Permissions.Doctors.Create",
            "Permissions.Doctors.Update",
            "Permissions.Roles.View",
            "Permissions.Roles.Create",
            "Permissions.Roles.Update",
            "Permissions.Bookings.View",
            "Permissions.Bookings.Update",
            "Permissions.Chats.View",
            "Permissions.Notifications.Send",
            "Permissions.Dashboard.View",
            "Permissions.Contacts.Manage",
        };

        await EnsureRoleWithPermissionsAsync(roleManager, context, "SuperAdmin", superAdminPermissions, isDefault: true);
        await EnsureRoleWithPermissionsAsync(roleManager, context, "Admin", adminPermissions, isDefault: true);

        context.ChangeTracker.Clear(); // Clear the tracker to detach roles before user seeding runs
    }

    private static async Task EnsureRoleWithPermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        AppDbContext context,
        string roleName,
        string[] desiredPermissions,
        bool isDefault)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        bool isNew = role is null;

        if (isNew)
        {
            role = new ApplicationRole { Name = roleName, IsDefault = isDefault };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
            }

            foreach (var permission in desiredPermissions)
                await roleManager.AddClaimAsync(role, new Claim("permission", permission));
        }
        else
        {
            bool needsRoleUpdate = role!.IsDefault != isDefault;

            var existingClaimValues = await context.Set<IdentityRoleClaim<string>>()
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
                .Select(rc => rc.ClaimValue!)
                .ToListAsync();

            bool permissionsMatch = desiredPermissions.ToHashSet().SetEquals(existingClaimValues);

            if (needsRoleUpdate || !permissionsMatch)
            {
                if (needsRoleUpdate)
                {
                    role.IsDefault = isDefault;
                    var result = await roleManager.UpdateAsync(role);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Failed to update role '{roleName}': {errors}");
                    }
                }

                if (!permissionsMatch)
                {
                    await context.Set<IdentityRoleClaim<string>>()
                        .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
                        .ExecuteDeleteAsync();

                    foreach (var permission in desiredPermissions)
                        await roleManager.AddClaimAsync(role, new Claim("permission", permission));
                }
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
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            if (role is not null && !await userManager.IsInRoleAsync(existingUser, role))
            {
                await userManager.AddToRoleAsync(existingUser, role);
            }
            return existingUser;
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

    private static async Task SeedNewsAsync(AppDbContext context)
    {
        if (!await context.HealthNews.AnyAsync())
        {
            var newsItems = new List<HealthNews>
            {
                new()
                {
                    Title = "Breakthrough in Heart Research",
                    Image = "https://img.freepik.com/free-vector/heart-anatomy-concept-illustration_114360-1014.jpg",
                    Description = "Scientists have discovered a new way to regenerate heart tissue using stem cells."
                },
                new()
                {
                    Title = "The Benefits of Daily Exercise",
                    Image = "https://img.freepik.com/free-photo/flat-lay-health-still-life-with-copy-space_23-2148854031.jpg",
                    Description = "Regular physical activity can significantly reduce the risk of chronic diseases."
                },
                new()
                {
                    Title = "New Mental Health Support App",
                    Image = "https://img.freepik.com/free-vector/mental-health-concept-illustration_114360-1014.jpg",
                    Description = "A revolutionary mobile application aims to provide 24/7 support for mental well-being."
                },
                new()
                {
                    Title = "Nutrition Tips for a Stronger Immune System",
                    Image = "https://img.freepik.com/free-photo/healthy-food-background_23-2148854031.jpg",
                    Description = "Learn which foods can help boost your body's natural defenses."
                }
            };

            context.HealthNews.AddRange(newsItems);
            await context.SaveChangesAsync();
            Console.WriteLine("Health news seeded successfully.");
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

    private static async Task SeedDashboardDataAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        // 1. Seed Patients
        var currentPatientCount = await context.PatientProfiles.CountAsync();
        if (currentPatientCount < 47)
        {
            var today = DateTime.UtcNow;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var earlierDate = lastMonthStart.AddMonths(-2);

            var currentMonthPatients = await context.PatientProfiles.CountAsync(p => p.CreatedAt >= currentMonthStart);
            var lastMonthPatients = await context.PatientProfiles.CountAsync(p => p.CreatedAt >= lastMonthStart && p.CreatedAt < currentMonthStart);
            var earlierPatients = currentPatientCount - currentMonthPatients - lastMonthPatients;

            int targetCurrentMonth = 6;
            int targetLastMonth = 5;
            int targetEarlier = 36;

            int toAddCurrentMonth = Math.Max(0, targetCurrentMonth - currentMonthPatients);
            int toAddLastMonth = Math.Max(0, targetLastMonth - lastMonthPatients);
            int toAddEarlier = Math.Max(0, targetEarlier - earlierPatients);

            // Seed earlier patients
            for (int i = 0; i < toAddEarlier; i++)
            {
                var email = $"patient.earlier.{i}@clinic.com";
                var user = await CreateUserAsync(userManager, $"Earlier Patient {i}", email, "Password123!");
                if (user != null && !await context.PatientProfiles.AnyAsync(p => p.Id == user.Id))
                {
                    context.PatientProfiles.Add(new PatientProfile
                    {
                        Id = user.Id,
                        Status = PatientStatus.ACTIVE,
                        BloodType = "A+",
                        CreatedAt = earlierDate.AddDays(i % 28)
                    });
                }
            }

            // Seed last month patients (June)
            for (int i = 0; i < toAddLastMonth; i++)
            {
                var email = $"patient.june.{i}@clinic.com";
                var user = await CreateUserAsync(userManager, $"June Patient {i}", email, "Password123!");
                if (user != null && !await context.PatientProfiles.AnyAsync(p => p.Id == user.Id))
                {
                    context.PatientProfiles.Add(new PatientProfile
                    {
                        Id = user.Id,
                        Status = PatientStatus.ACTIVE,
                        BloodType = "B+",
                        CreatedAt = lastMonthStart.AddDays(i * 5)
                    });
                }
            }

            // Seed current month patients (July)
            for (int i = 0; i < toAddCurrentMonth; i++)
            {
                var email = $"patient.july.{i}@clinic.com";
                var user = await CreateUserAsync(userManager, $"July Patient {i}", email, "Password123!");
                if (user != null && !await context.PatientProfiles.AnyAsync(p => p.Id == user.Id))
                {
                    context.PatientProfiles.Add(new PatientProfile
                    {
                        Id = user.Id,
                        Status = PatientStatus.ACTIVE,
                        BloodType = "O-",
                        CreatedAt = currentMonthStart.AddDays(i * 3)
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        // 2. Seed Bookings
        if (!await context.Bookings.AnyAsync())
        {
            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(d => d.Status == DoctorStatus.ACTIVE);
            if (doctor == null) return;

            var patients = await context.PatientProfiles.Take(47).ToListAsync();
            if (!patients.Any()) return;

            var today = DateTime.UtcNow;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var performance = new[]
            {
                new { MonthOffset = -5, Completed = 18, Canceled = 3 }, // Feb 2026
                new { MonthOffset = -4, Completed = 22, Canceled = 2 }, // Mar 2026
                new { MonthOffset = -3, Completed = 20, Canceled = 4 }, // Apr 2026
                new { MonthOffset = -2, Completed = 25, Canceled = 1 }, // May 2026
                new { MonthOffset = -1, Completed = 19, Canceled = 3 }, // Jun 2026
                new { MonthOffset = 0,  Completed = 15, Canceled = 2 }  // Jul 2026
            };

            int bookingIndex = 0;
            foreach (var perf in performance)
            {
                var monthStart = currentMonthStart.AddMonths(perf.MonthOffset);
                var dateOnlyStart = DateOnly.FromDateTime(monthStart);

                for (int day = 1; day <= Math.Max(perf.Completed, perf.Canceled); day++)
                {
                    var scheduleDate = dateOnlyStart.AddDays(day - 1);
                    var schedule = await context.DoctorSchedules
                        .FirstOrDefaultAsync(ds => ds.DoctorId == doctor.Id && ds.Date == scheduleDate);

                    if (schedule == null)
                    {
                        schedule = new DoctorSchedule
                        {
                            DoctorId = doctor.Id,
                            Date = scheduleDate,
                            IsAvailable = true,
                            BookingCount = 0
                        };
                        context.DoctorSchedules.Add(schedule);
                    }

                    if (day <= perf.Completed)
                    {
                        var patient = patients[bookingIndex % patients.Count];
                        context.Bookings.Add(new Booking
                        {
                            PatientId = patient.Id,
                            DoctorSchedule = schedule,
                            Status = BookingStatus.COMPLETED
                        });
                        schedule.BookingCount++;
                        bookingIndex++;
                    }

                    if (day <= perf.Canceled)
                    {
                        var patient = patients[bookingIndex % patients.Count];
                        context.Bookings.Add(new Booking
                        {
                            PatientId = patient.Id,
                            DoctorSchedule = schedule,
                            Status = BookingStatus.CANCELLED
                        });
                        schedule.BookingCount++;
                        bookingIndex++;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
