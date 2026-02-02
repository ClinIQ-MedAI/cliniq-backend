using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Contracts.Doctors;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Errors;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Profile.Doctor.Services;

/// <summary>
/// Doctor survey service.
/// Creates DoctorProfile from survey submission.
/// Registration is handled by the shared RegistrationService.
/// </summary>
public class DoctorRegistrationService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Creates DoctorProfile from survey submission.
    /// Requires verified user (email or phone).
    /// </summary>
    public async Task<Result> SubmitSurveyAsync(string userId, DoctorSurveyRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        // Check if user is verified
        if (!user!.EmailConfirmed && !user.PhoneNumberConfirmed)
            return Result.Failure(new Error("User.NotVerified", "Please verify email or phone before submitting survey", StatusCodes.Status403Forbidden));

        // Check if doctor profile already exists
        var existingProfile = await _context.DoctorProfiles.FindAsync([userId], cancellationToken);
        if (existingProfile is not null)
            return Result.Failure(new Error("Doctor.ProfileExists", "Doctor profile already exists", StatusCodes.Status409Conflict));

        // Create DoctorProfile with Shared PK
        var doctorProfile = new DoctorProfile
        {
            Id = userId,
            Status = DoctorStatus.PENDING_VERIFICATION,
            PersonalIdentityPhotoUrl = request.PersonalIdentityPhotoUrl,
            MedicalLicenseUrl = request.MedicalLicenseUrl,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            LicenseExpiryDate = request.LicenseExpiryDate
        };

        _context.DoctorProfiles.Add(doctorProfile);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Succeed();
    }
}
