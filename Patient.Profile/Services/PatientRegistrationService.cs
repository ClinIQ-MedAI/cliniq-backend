using System.Security.Claims;
using Clinic.Authentication.Authorization;
using Clinic.Authentication.Contracts;
using Clinic.Authentication.Jwt;
using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Patient.Profile.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Patient.Profile.Services;

/// <summary>
/// Patient survey service.
/// Creates PatientProfile from survey submission.
/// Registration is handled by the shared RegistrationService.
/// </summary>
public class PatientRegistrationService(
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
    /// Creates PatientProfile from survey submission.
    /// Requires verified user (email or phone).
    /// </summary>
    public async Task<Result<LoginResponse>> SubmitSurveyAsync(string userId, PatientSurveyRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        // Check if user is verified
        if (!user!.EmailConfirmed && !user.PhoneNumberConfirmed)
            return Result.Failure<LoginResponse>(new Error("User.NotVerified", _localizer["UserNotVerified"], StatusCodes.Status403Forbidden));

        // Check if patient profile already exists
        var existingProfile = await _context.PatientProfiles.FindAsync([userId], cancellationToken);
        if (existingProfile is not null)
            return Result.Failure<LoginResponse>(new Error("Patient.ProfileExists", _localizer["ProfileExists"], StatusCodes.Status409Conflict));

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

        await _notificationService.NotifyAdminsAsync(
            "New Patient Registration",
            $"{user.FirstName} {user.LastName} has completed their profile.",
            NotificationType.PATIENT_NEW_REGISTRATION,
            userId
        );

        // Load doctor status if any
        var doctorEntity = await _context.DoctorProfiles
            .Where(d => d.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);
        var doctorStatus = doctorEntity?.Status ?? DoctorStatus.INCOMPLETE_PROFILE;

        var patientInfo = new PatientInfo(
            patientProfile.Status.ToString(),
            patientProfile.Height,
            patientProfile.Weight,
            patientProfile.BloodType,
            patientProfile.Allergies,
            patientProfile.ChronicConditions,
            patientProfile.EmergencyContactName,
            patientProfile.EmergencyContactPhone
        );

        DoctorInfo? doctorInfo = doctorEntity is null ? null : new DoctorInfo(
            doctorEntity.Status.ToString(),
            doctorEntity.Specialization,
            doctorEntity.LicenseNumber,
            doctorEntity.LicenseExpiryDate,
            doctorEntity.PersonalIdentityPhotoUrl,
            doctorEntity.MedicalLicenseUrl,
            doctorEntity.RejectionReason
        );

        // Load roles and permissions
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var permissions = (await _permissionService.GetPermissionsForUserAsync(
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Id) }
                .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)))))))
            .ToList();
        AdminInfo? adminInfo = roles.Count > 0 ? new AdminInfo(roles, permissions) : null;

        // Generate JWT token
        var (token, expiresAt) = await _jwtProvider.GenerateTokenAsync(user, PatientStatus.ACTIVE, doctorStatus);
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
