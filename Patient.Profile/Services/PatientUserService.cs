using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Patient.Profile.Services;

public class PatientUserService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context) : IPatientUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    public async Task<Result<PatientProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var patientProfile = await _context.PatientProfiles
            .Where(p => p.Id == userId)
            .FirstOrDefaultAsync();

        var response = new PatientProfileResponse(
            userId,
            user!.Email!,
            user.UserName!,
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.Gender?.ToString(),
            patientProfile?.Status.ToString() ?? PatientStatus.INCOMPLETE_PROFILE.ToString(),
            patientProfile?.Height,
            patientProfile?.Weight,
            patientProfile?.HasDiabetes ?? false,
            patientProfile?.HasPressureIssues ?? false,
            patientProfile?.BloodType,
            patientProfile?.Allergies,
            patientProfile?.ChronicConditions,
            patientProfile?.EmergencyContactName,
            patientProfile?.EmergencyContactPhone
        );

        return Result.Succeed(response);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _userManager.UpdateAsync(user);

        var patientProfile = await _context.PatientProfiles.FindAsync([userId]);

        if (patientProfile is not null)
        {
            if (request.Height.HasValue) patientProfile.Height = request.Height;
            if (request.Weight.HasValue) patientProfile.Weight = request.Weight;
            if (request.HasDiabetes.HasValue) patientProfile.HasDiabetes = request.HasDiabetes.Value;
            if (request.HasPressureIssues.HasValue) patientProfile.HasPressureIssues = request.HasPressureIssues.Value;
            if (request.BloodType is not null) patientProfile.BloodType = request.BloodType;
            if (request.Allergies is not null) patientProfile.Allergies = request.Allergies;
            if (request.ChronicConditions is not null) patientProfile.ChronicConditions = request.ChronicConditions;
            if (request.EmergencyContactName is not null) patientProfile.EmergencyContactName = request.EmergencyContactName;
            if (request.EmergencyContactPhone is not null) patientProfile.EmergencyContactPhone = request.EmergencyContactPhone;

            await _context.SaveChangesAsync();
        }

        return Result.Succeed();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Succeed();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
