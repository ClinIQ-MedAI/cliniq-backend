using Clinic.Infrastructure.Contracts.Doctors;
using Clinic.Authentication.Contracts.Users;

namespace DashboardAPI.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<DoctorResponse>> GetAsync(string id);
    Task<Result<DoctorResponse>> AddAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(string id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> ToggleStatus(string id);
    Task<Result> Unlock(string id);
    Task<Result<DoctorProfileResponse>> GetProfileAsync(string doctorId);
    Task<Result> UpdateProfileAsync(string doctorId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string doctorId, ChangePasswordRequest request);
}


