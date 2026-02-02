using Clinic.Infrastructure.Contracts.Doctors;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Mapster;

namespace Management.Doctor.Services;

public class DoctorService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context) : IDoctorService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Fetch all doctor profiles and join with ApplicationUser
        var doctorProfiles = await _context.DoctorProfiles
            .Include(dp => dp.User)
            .ToListAsync(cancellationToken);

        var result = doctorProfiles.Select(dp => new DoctorResponse(
            dp.Id,
            dp.User.FirstName,
            dp.User.LastName,
            dp.User.Email!,
            dp.User.IsDisabled,
            dp.Status
        ));

        return result;
    }

    public async Task<Result<DoctorResponse>> GetAsync(string id)
    {
        var doctorProfile = await _context.DoctorProfiles
            .Include(dp => dp.User)
            .FirstOrDefaultAsync(dp => dp.Id == id);

        if (doctorProfile == null)
            return Result.Failure<DoctorResponse>(Error.Failure("Doctor.NotFound", "Doctor profile not found"));

        var response = new DoctorResponse(
            doctorProfile.Id,
            doctorProfile.User.FirstName,
            doctorProfile.User.LastName,
            doctorProfile.User.Email!,
            doctorProfile.User.IsDisabled,
            doctorProfile.Status
        );

        return Result.Succeed(response);
    }

    public async Task<Result<DoctorResponse>> AddAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<DoctorResponse>(Error.Failure("User.DuplicateEmail", "Email already exists"));

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;
        user.IsDisabled = false;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;

        // Create Doctor Profile with provided fields
        user.DoctorProfile = new DoctorProfile
        {
            Status = DoctorStatus.ACTIVE,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var response = new DoctorResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.IsDisabled,
                user.DoctorProfile.Status
            );

            return Result.Succeed(response);
        }

        var error = result.Errors.First();
        return Result.Failure<DoctorResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var doctorProfile = await _context.DoctorProfiles
            .Include(dp => dp.User)
            .FirstOrDefaultAsync(dp => dp.Id == id, cancellationToken);

        if (doctorProfile == null)
            return Result.Failure(Error.Failure("Doctor.NotFound", "Doctor profile not found"));

        var user = doctorProfile.User;

        // Email check (only if email is being updated)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var userWithEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithEmail != null && userWithEmail.Id != id)
                return Result.Failure(Error.Failure("User.DuplicateEmail", "Email already exists"));

            user.Email = request.Email;
            user.UserName = request.Email;
        }

        // Update user info (only if provided)
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;
        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;
        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth;
        if (request.Gender.HasValue)
            user.Gender = request.Gender;

        // Update profile fields
        if (!string.IsNullOrWhiteSpace(request.Specialization))
            doctorProfile.Specialization = request.Specialization;
        if (!string.IsNullOrWhiteSpace(request.LicenseNumber))
            doctorProfile.LicenseNumber = request.LicenseNumber;
        if (request.Status.HasValue)
            doctorProfile.Status = request.Status.Value;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Succeed();
        }

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateStatusAsync(string id, bool active)
    {
        var doctorProfile = await _context.DoctorProfiles
            .FirstOrDefaultAsync(dp => dp.Id == id);

        if (doctorProfile == null)
            return Result.Failure(Error.Failure("Doctor.NotFound", "Doctor profile not found"));

        // Only allow toggling between ACTIVE and SUSPENDED
        if (active)
        {
            if (doctorProfile.Status != DoctorStatus.SUSPENDED)
                return Result.Failure(Error.Failure("Doctor.InvalidStatus", "Can only reactivate a suspended doctor"));

            doctorProfile.Status = DoctorStatus.ACTIVE;
        }
        else
        {
            if (doctorProfile.Status != DoctorStatus.ACTIVE)
                return Result.Failure(Error.Failure("Doctor.InvalidStatus", "Can only suspend an active doctor"));

            doctorProfile.Status = DoctorStatus.SUSPENDED;
        }

        await _context.SaveChangesAsync();
        return Result.Succeed();
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

    public async Task<Result> AcceptAsync(string id)
    {
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.Id == id);
        if (doctorProfile == null)
            return Result.Failure(Error.Failure("Doctor.NotFound", "Doctor profile not found"));

        if (doctorProfile.Status != DoctorStatus.PENDING_VERIFICATION)
            return Result.Failure(Error.Failure("Doctor.InvalidStatus", "Can only accept a pending doctor"));

        doctorProfile.Status = DoctorStatus.ACTIVE;
        doctorProfile.RejectionReason = null;

        await _context.SaveChangesAsync();
        return Result.Succeed();
    }

    public async Task<Result> RejectAsync(string id, string reason)
    {
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.Id == id);
        if (doctorProfile == null)
            return Result.Failure(Error.Failure("Doctor.NotFound", "Doctor profile not found"));

        if (doctorProfile.Status != DoctorStatus.PENDING_VERIFICATION)
            return Result.Failure(Error.Failure("Doctor.InvalidStatus", "Can only reject a pending doctor"));

        doctorProfile.Status = DoctorStatus.REJECTED;
        doctorProfile.RejectionReason = reason;

        await _context.SaveChangesAsync();
        return Result.Succeed();
    }
}
