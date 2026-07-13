using System.Security.Claims;
using Clinic.Authentication.Authorization;
using Clinic.Authentication.Contracts;
using Clinic.Authentication.Jwt;
using Clinic.Infrastructure.Contracts.Doctors;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Doctor.Profile.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Doctor.Profile.Services;

/// <summary>
/// Doctor survey service.
/// Creates DoctorProfile from survey submission.
/// Registration is handled by the shared RegistrationService.
/// </summary>
public class DoctorRegistrationService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    INotificationService notificationService,
    IJwtProvider jwtProvider,
    IAuthPermissionService permissionService,
    IStringLocalizer<Messages> localizer)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IAuthPermissionService _permissionService = permissionService;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    /// <summary>
    /// Creates DoctorProfile from survey submission.
    /// Requires verified user (email or phone).
    /// </summary>
    public async Task<Result<LoginResponse>> SubmitSurveyAsync(string userId, DoctorSurveyRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        // Check if user is verified
        if (!user!.EmailConfirmed && !user.PhoneNumberConfirmed)
            return Result.Failure<LoginResponse>(new Error("User.NotVerified", _localizer["UserNotVerified"], StatusCodes.Status403Forbidden));

        // Check if doctor profile already exists
        var existingProfile = await _context.DoctorProfiles.FindAsync([userId], cancellationToken);
        if (existingProfile is not null)
            return Result.Failure<LoginResponse>(new Error("Doctor.ProfileExists", _localizer["ProfileExists"], StatusCodes.Status409Conflict));

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

        await _notificationService.NotifyAdminsAsync(
            "New Doctor Join Request",
            $"Dr. {user.FirstName} {user.LastName} has submitted their profile for verification.",
            NotificationType.DOCTOR_JOIN_REQUEST,
            userId
        );

        // Load patient status if any
        var patientEntity = await _context.PatientProfiles
            .Where(p => p.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);
        var patientStatus = patientEntity?.Status ?? PatientStatus.INCOMPLETE_PROFILE;

        PatientInfo? patientInfo = patientEntity is null ? null : new PatientInfo(
            patientEntity.Status.ToString(),
            patientEntity.Height,
            patientEntity.Weight,
            patientEntity.BloodType,
            patientEntity.Allergies,
            patientEntity.ChronicConditions,
            patientEntity.EmergencyContactName,
            patientEntity.EmergencyContactPhone
        );

        var doctorInfo = new DoctorInfo(
            doctorProfile.Status.ToString(),
            doctorProfile.Specialization,
            doctorProfile.LicenseNumber,
            doctorProfile.LicenseExpiryDate,
            doctorProfile.PersonalIdentityPhotoUrl,
            doctorProfile.MedicalLicenseUrl,
            doctorProfile.RejectionReason
        );

        // Load roles and permissions
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var permissions = (await _permissionService.GetPermissionsForUserAsync(
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Id) }
                .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)))))))
            .ToList();
        AdminInfo? adminInfo = roles.Count > 0 ? new AdminInfo(roles, permissions) : null;

        // Generate JWT token
        var (token, expiresAt) = await _jwtProvider.GenerateTokenAsync(user, patientStatus, DoctorStatus.PENDING_VERIFICATION);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        var response = new LoginResponse(
            token,
            refreshToken,
            expiresAt,
            new UserInfo(
                user.Id,
                user.Email!,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.PhoneNumberConfirmed
            ),
            doctorInfo,
            patientInfo,
            adminInfo
        );

        return Result.Succeed(response);
    }
}
