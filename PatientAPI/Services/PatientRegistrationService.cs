using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Errors;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace PatientAPI.Services;

/// <summary>
/// Patient survey service.
/// Creates PatientProfile from survey submission.
/// Registration is handled by the shared RegistrationService.
/// </summary>
public class PatientRegistrationService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Creates PatientProfile from survey submission.
    /// Requires verified user (email or phone).
    /// </summary>
    public async Task<Result> SubmitSurveyAsync(string userId, PatientSurveyRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        // Check if user is verified
        if (!user.EmailVerified && !user.PhoneVerified)
            return Result.Failure(new Error("User.NotVerified", "Please verify email or phone before submitting survey", StatusCodes.Status403Forbidden));

        // Check if patient profile already exists
        var existingProfile = await _context.PatientProfiles.FindAsync([userId], cancellationToken);
        if (existingProfile is not null)
            return Result.Failure(new Error("Patient.ProfileExists", "Patient profile already exists", StatusCodes.Status409Conflict));

        // Create PatientProfile with Shared PK
        var patientProfile = new PatientProfile
        {
            Id = userId,
            Status = PatientStatus.ACTIVE,  // Patient profiles are immediately active
            Height = request.Height,
            Weight = request.Weight,
            HasDiabetes = request.HasDiabetes,
            HasPressureIssues = request.HasPressureIssues,
            BloodType = request.BloodType,
            Allergies = request.Allergies,
            ChronicConditions = request.ChronicConditions,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone
        };

        _context.PatientProfiles.Add(patientProfile);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            throw;
        }

        return Result.Succeed();
    }
}
