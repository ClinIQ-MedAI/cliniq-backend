using Clinic.Authentication.Authorization;
using Clinic.Authentication.Contracts;
using Clinic.Authentication.Jwt;
using Clinic.Authentication.Strategies;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Clinic.Authentication.Localization;

namespace Clinic.Authentication.Services;

/// <summary>
/// Authentication service that handles login via password or OTP.
/// </summary>
public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    IJwtProvider jwtProvider,
    IOtpService otpService,
    IAuthPermissionService permissionService,
    IEnumerable<ILoginStrategy> loginStrategies,
    IStringLocalizer<Messages> localizer) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IOtpService _otpService = otpService;
    private readonly IAuthPermissionService _permissionService = permissionService;
    private readonly IEnumerable<ILoginStrategy> _loginStrategies = loginStrategies;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure<LoginResponse>(Error.BadRequest("Auth.InvalidCredentials", _localizer["InvalidCredentials"]));

        // Check if user is verified
        if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
            return Result.Failure<LoginResponse>(Error.BadRequest("Auth.NotVerified", _localizer["NotVerified"]));

        // Select appropriate login strategy
        ILoginStrategy? strategy = _loginStrategies.FirstOrDefault(s => s.CanHandle(request));
        if (strategy == null)
            return Result.Failure<LoginResponse>(Error.BadRequest("Auth.InvalidLoginRequest", _localizer["InvalidLoginRequest"]));

        // Validate credentials using strategy
        var isValid = await strategy.ValidateAsync(user, request, cancellationToken);
        if (!isValid)
            return Result.Failure<LoginResponse>(Error.BadRequest("Auth.InvalidCredentials", _localizer["InvalidCredentials"]));

        // Load profiles
        var patientEntity = await _context.PatientProfiles
            .Where(p => p.Id == user.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var doctorEntity = await _context.DoctorProfiles
            .Where(d => d.Id == user.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var patientStatus = patientEntity?.Status ?? PatientStatus.INCOMPLETE_PROFILE;
        var doctorStatus = doctorEntity?.Status ?? DoctorStatus.INCOMPLETE_PROFILE;

        PatientInfo? patientProfile = patientEntity is null ? null : new PatientInfo(
            patientEntity.Status.ToString(),
            patientEntity.Height,
            patientEntity.Weight,
            patientEntity.BloodType,
            patientEntity.Allergies,
            patientEntity.ChronicConditions,
            patientEntity.EmergencyContactName,
            patientEntity.EmergencyContactPhone
        );

        DoctorInfo? doctorProfile = doctorEntity is null ? null : new DoctorInfo(
            doctorEntity.Status.ToString(),
            doctorEntity.Specialization,
            doctorEntity.LicenseNumber,
            doctorEntity.LicenseExpiryDate,
            doctorEntity.PersonalIdentityPhotoUrl,
            doctorEntity.MedicalLicenseUrl,
            doctorEntity.RejectionReason
        );

        // Load roles
        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        // Load permissions
        var permissions = (await _permissionService.GetPermissionsForUserAsync(
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Id) }
                .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)))))))
            .ToList();

        // Generate JWT token
        var (token, expiresAt) = await _jwtProvider.GenerateTokenAsync(user, patientStatus, doctorStatus);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        AdminInfo? adminInfo = roles.Count > 0 ? new AdminInfo(roles, permissions) : null;

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
            doctorProfile,
            patientProfile,
            adminInfo
        );

        return Result.Succeed(response);
    }

    public async Task<Result> SendLoginOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure(Error.NotFound("User.NotFound", _localizer["UserNotFound"]));

        // Require verified email or phone before allowing OTP login
        if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
            return Result.Failure(Error.BadRequest("Auth.NotVerified", _localizer["NotVerified"]));

        // Generate and store OTP for login context
        await _otpService.GenerateAndStoreAsync(
            OtpContext.Login,
            OtpIdentifierType.Email,
            email,
            cancellationToken);

        // TODO: Send OTP via email service

        return Result.Succeed();
    }
}
