using Clinic.Infrastructure.Contracts.Doctors;

namespace Doctor.Profile.Services;

public interface IUserService
{
    Task<Result<DoctorProfileResponse>> GetProfileAsync(string userId);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<Result<DoctorProfileUpdateRequestResponse>> SubmitUpdateRequestAsync(string userId, SubmitDoctorUpdateRequestRequest request, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
