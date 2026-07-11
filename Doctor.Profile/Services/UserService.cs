using Clinic.Infrastructure.Contracts.Doctors;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Doctor.Profile.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    INotificationService notificationService) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result<DoctorProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var doctorProfile = await _context.DoctorProfiles
            .Where(d => d.Id == userId)
            .FirstOrDefaultAsync();

        var pendingRequest = await _context.DoctorProfileUpdateRequests
            .Where(r => r.DoctorId == userId && r.Status == DoctorProfileUpdateRequestStatus.PENDING)
            .Select(r => new DoctorProfileUpdateRequestResponse(
                r.Id,
                r.DoctorId,
                r.Status,
                r.RejectionReason,
                r.Specialization,
                r.LicenseNumber,
                r.LicenseExpiryDate,
                r.PersonalIdentityPhotoUrl,
                r.MedicalLicenseUrl,
                r.CreatedAt
            ))
            .FirstOrDefaultAsync();

        var response = new DoctorProfileResponse(
            user!.Email!,
            user.UserName!,
            user.FirstName,
            user.LastName,
            doctorProfile?.Specialization,
            doctorProfile?.LicenseNumber,
            doctorProfile?.LicenseExpiryDate,
            doctorProfile?.PersonalIdentityPhotoUrl,
            doctorProfile?.MedicalLicenseUrl,
            doctorProfile?.RejectionReason,
            doctorProfile?.Status.ToString() ?? DoctorStatus.INCOMPLETE_PROFILE.ToString(),
            pendingRequest
        );

        return Result.Succeed(response);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _userManager.UpdateAsync(user);

        return Result.Succeed();
    }

    public async Task<Result<DoctorProfileUpdateRequestResponse>> SubmitUpdateRequestAsync(string userId, SubmitDoctorUpdateRequestRequest request, CancellationToken cancellationToken)
    {
        var doctorProfile = await _context.DoctorProfiles.FindAsync([userId], cancellationToken);

        if (doctorProfile is null)
            return Result.Failure<DoctorProfileUpdateRequestResponse>(Error.BadRequest("Doctor.ProfileNotFound", "Doctor profile not found"));

        // Check if there's already a pending request
        var existingPending = await _context.DoctorProfileUpdateRequests
            .AnyAsync(r => r.DoctorId == userId && r.Status == DoctorProfileUpdateRequestStatus.PENDING, cancellationToken);

        if (existingPending)
            return Result.Failure<DoctorProfileUpdateRequestResponse>(Error.BadRequest("Doctor.PendingRequestExists", "A pending update request already exists"));

        var updateRequest = new DoctorProfileUpdateRequest
        {
            DoctorId = userId,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            LicenseExpiryDate = request.LicenseExpiryDate,
            PersonalIdentityPhotoUrl = request.PersonalIdentityPhotoUrl,
            MedicalLicenseUrl = request.MedicalLicenseUrl
        };

        _context.DoctorProfileUpdateRequests.Add(updateRequest);
        await _context.SaveChangesAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userId);

        await _notificationService.NotifyAdminsAsync(
            "Doctor Profile Update Request",
            $"Dr. {user!.FirstName} {user.LastName} has submitted a profile update request.",
            NotificationType.DOCTOR_PROFILE_UPDATE_REQUEST,
            userId
        );

        var response = new DoctorProfileUpdateRequestResponse(
            updateRequest.Id,
            updateRequest.DoctorId,
            updateRequest.Status,
            updateRequest.RejectionReason,
            updateRequest.Specialization,
            updateRequest.LicenseNumber,
            updateRequest.LicenseExpiryDate,
            updateRequest.PersonalIdentityPhotoUrl,
            updateRequest.MedicalLicenseUrl,
            updateRequest.CreatedAt
        );

        return Result.Succeed(response);
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
