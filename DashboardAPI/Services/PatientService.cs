using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Infrastructure.Contracts.Users; // For UpdateProfileRequest/ChangePassword
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Errors;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace DashboardAPI.Services;

public class PatientService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    AppDbContext context) : IPatientService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Get users who are in 'Patient' role? Or just users who have PatientProfile?
        // Querying by role is cleaner if roles are consistently assigned.
        // Assuming we have a "Patient" role.

        var users = await _userManager.GetUsersInRoleAsync("Patient");
        // GetUsersInRoleAsync doesn't support generic filtering easily or projection.

        // Better to query directly joined with roles
        var query = from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Name == "Patient" // Or check PatientProfile existence
                    select new { User = u, Roles = _context.UserRoles.Where(x => x.UserId == u.Id).Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToList() };

        // For simplicity, let's just fetch all users for now and filter in memory if volume is low, 
        // OR better: use EF projection.
        // Given existing code logic:

        var patientRoleId = await _context.Roles.Where(r => r.Name == "Patient").Select(r => r.Id).FirstOrDefaultAsync(cancellationToken);

        var patients = await _context.Users
            .Where(u => u.PatientProfile != null) // Users who have patient profile
            .Select(u => new PatientResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email!,
                u.IsDisabled,
                _context.UserRoles.Where(ur => ur.UserId == u.Id).Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!).ToList()
            ))
            .ToListAsync(cancellationToken);

        return patients;
    }

    public async Task<Result<PatientResponse>> GetAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure<PatientResponse>(UserErrors.UserNotFound);

        var roles = await _userManager.GetRolesAsync(user);

        return Result.Succeed(new PatientResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsDisabled,
            roles
        ));
    }

    public async Task<Result<PatientResponse>> AddAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userManager.FindByEmailAsync(request.Email);
        if (emailExists is not null)
            return Result.Failure<PatientResponse>(UserErrors.EmailDuplicated);

        // Validation of roles?
        // assuming standard roles

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            // Create Profile
            _context.PatientProfiles.Add(new PatientProfile { Id = user.Id });
            await _context.SaveChangesAsync(cancellationToken);

            // Assign Roles
            if (request.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Patient");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Result.Succeed(new PatientResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.IsDisabled,
                roles));
        }

        var error = result.Errors.First();
        return Result.Failure<PatientResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // Sync roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, request.Roles);

            return Result.Succeed();
        }

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;

        await _userManager.UpdateAsync(user);
        return Result.Succeed();
    }

    public async Task<Result> Unlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        await _userManager.SetLockoutEndDateAsync(user, null);
        return Result.Succeed();
    }

    public async Task<Result<PatientProfileResponse>> GetProfileAsync(string patientId)
    {
        var user = await _userManager.FindByIdAsync(patientId);
        if (user is null) return Result.Failure<PatientProfileResponse>(UserErrors.UserNotFound);

        // This should fetch actual profile data if we had birthday/gender in PatientProfile
        // For now, mapping from User + Profile
        var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.Id == patientId);
        if (profile == null) return Result.Failure<PatientProfileResponse>(UserErrors.UserNotFound); // Profile missing

        var response = new PatientProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            DateOnly.MinValue, // Placeholder
            "Unknown" // Placeholder
        );

        return Result.Succeed(response);
    }

    public async Task<Result> UpdateProfileAsync(string patientId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(patientId);
        if (user is null) return Result.Failure(UserErrors.UserNotFound);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        // Update profile fields if we had them

        await _userManager.UpdateAsync(user);
        return Result.Succeed();
    }

    public async Task<Result> ChangePasswordAsync(string patientId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(patientId);
        if (user is null) return Result.Failure(UserErrors.UserNotFound);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Succeed();
    }
}


