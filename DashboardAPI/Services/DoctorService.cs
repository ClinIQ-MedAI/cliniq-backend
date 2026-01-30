using Clinic.Infrastructure.Contracts.Doctors;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Abstractions.Consts;
using Clinic.Authentication.Contracts.Users;

namespace DashboardAPI.Services;

public class DoctorService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    AppDbContext context) : IDoctorService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Get all users who have a DoctorProfile (or check roles)
        // For efficiency, we can join Users, UserRoles, Roles
        var query = from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    // Filter where role is NOT Member (Patient) - simplistic view, or just get all for now
                    // Ideally, filter by "Doctor" role if it exists, or check absence of specific roles.
                    // The original code filtered out "Member".
                    where r.Name != DefaultRoles.Member
                    select new { User = u, RoleName = r.Name };

        var data = await query.ToListAsync(cancellationToken);

        var grouped = data.GroupBy(x => x.User);

        var result = new List<DoctorResponse>();

        foreach (var group in grouped)
        {
            var user = group.Key;
            var roles = group.Select(x => x.RoleName).ToList();

            result.Add(new DoctorResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.IsDisabled, // IsDisabled
                roles!           // Roles
            ));
        }

        return result;
    }

    public async Task<Result<DoctorResponse>> GetAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<DoctorResponse>(Error.Failure("User.NotFound", "User not found"));

        var roles = await _userManager.GetRolesAsync(user);

        var response = new DoctorResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsDisabled,
            roles
        );

        return Result.Succeed(response);
    }

    public async Task<Result<DoctorResponse>> AddAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<DoctorResponse>(Error.Failure("User.DuplicateEmail", "Email already exists"));

        // Validate roles
        foreach (var roleName in request.Roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                return Result.Failure<DoctorResponse>(Error.Failure("Role.NotFound", $"Role '{roleName}' not found"));
        }

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;
        user.IsDisabled = false;

        // Create Doctor Profile
        user.DoctorProfile = new DoctorProfile
        {
            // Initialize specific doctor properties if any
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, request.Roles);

            var response = new DoctorResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.IsDisabled,
                request.Roles
            );

            return Result.Succeed(response);
        }

        var error = result.Errors.First();
        return Result.Failure<DoctorResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        // Email check
        var userWithEmail = await _userManager.FindByEmailAsync(request.Email);
        if (userWithEmail != null && userWithEmail.Id != id)
            return Result.Failure(Error.Failure("User.DuplicateEmail", "Email already exists"));

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // Update roles
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
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        user.IsDisabled = !user.IsDisabled;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded
            ? Result.Succeed()
            : Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> Unlock(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        return result.Succeeded
            ? Result.Succeed()
            : Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<DoctorProfileResponse>> GetProfileAsync(string doctorId)
    {
        var doctorProfile = await _context.DoctorProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == doctorId);

        if (doctorProfile == null)
            return Result.Failure<DoctorProfileResponse>(Error.Failure("Doctor.NotFound", "Doctor profile not found"));

        // Map to response
        var response = new DoctorProfileResponse(
            doctorProfile.Id,
            doctorProfile.User.FirstName,
            doctorProfile.User.LastName,
            doctorProfile.User.Email!
        // Add other props
        );

        return Result.Succeed(response);
    }

    public async Task<Result> UpdateProfileAsync(string doctorId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(doctorId);
        if (user == null) return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
           ? Result.Succeed()
           : Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ChangePasswordAsync(string doctorId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(doctorId);
        if (user == null) return Result.Failure(Error.Failure("User.NotFound", "User not found"));

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return result.Succeeded
           ? Result.Succeed()
           : Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
    }
}
