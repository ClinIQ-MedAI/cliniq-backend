using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;

namespace Management.Patient.Services;

public class PatientService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context) : IPatientService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Fetch all patient profiles and join with ApplicationUser
        var patientProfiles = await _context.PatientProfiles
            .Include(pp => pp.User)
            .ToListAsync(cancellationToken);

        var result = patientProfiles.Select(pp => new PatientResponse(
            pp.Id,
            pp.User.FirstName,
            pp.User.LastName,
            pp.User.Email!,
            pp.User.IsDisabled,
            pp.Status
        ));

        return result;
    }

    public async Task<Result<PatientResponse>> GetAsync(string id)
    {
        var patientProfile = await _context.PatientProfiles
            .Include(pp => pp.User)
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (patientProfile == null)
            return Result.Failure<PatientResponse>(Error.Failure("Patient.NotFound", "Patient profile not found"));

        var response = new PatientResponse(
            patientProfile.Id,
            patientProfile.User.FirstName,
            patientProfile.User.LastName,
            patientProfile.User.Email!,
            patientProfile.User.IsDisabled,
            patientProfile.Status
        );

        return Result.Succeed(response);
    }

    public async Task<Result<PatientResponse>> AddAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<PatientResponse>(Error.Failure("User.DuplicateEmail", "Email already exists"));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            EmailConfirmed = true
        };

        // Create Patient Profile
        user.PatientProfile = new PatientProfile
        {
            Status = PatientStatus.ACTIVE
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var response = new PatientResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.IsDisabled,
                user.PatientProfile.Status
            );

            return Result.Succeed(response);
        }

        var error = result.Errors.First();
        return Result.Failure<PatientResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patientProfile = await _context.PatientProfiles
            .Include(pp => pp.User)
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);

        if (patientProfile == null)
            return Result.Failure(Error.Failure("Patient.NotFound", "Patient profile not found"));

        var user = patientProfile.User;

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
        var patientProfile = await _context.PatientProfiles
            .FirstOrDefaultAsync(pp => pp.Id == id);

        if (patientProfile == null)
            return Result.Failure(Error.Failure("Patient.NotFound", "Patient profile not found"));

        // Only allow toggling between ACTIVE and SUSPENDED
        if (active)
        {
            if (patientProfile.Status != PatientStatus.SUSPENDED)
                return Result.Failure(Error.Failure("Patient.InvalidStatus", "Can only reactivate a suspended patient"));

            patientProfile.Status = PatientStatus.ACTIVE;
        }
        else
        {
            if (patientProfile.Status != PatientStatus.ACTIVE)
                return Result.Failure(Error.Failure("Patient.InvalidStatus", "Can only suspend an active patient"));

            patientProfile.Status = PatientStatus.SUSPENDED;
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
}
