using Clinic.Infrastructure.Contracts.Doctors;

namespace Doctor.Profile.Services;

public interface IDoctorUserService
{
    Task<Result<DoctorProfileResponse>> GetProfileAsync(string userId);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<Result<DoctorProfileUpdateRequestResponse>> SubmitUpdateRequestAsync(string userId, SubmitDoctorUpdateRequest request, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
