using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Contracts.Patients;
using Clinic.Authentication.Contracts.Users;

namespace DashboardAPI.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PatientResponse>> GetAsync(string id);
    Task<Result<PatientResponse>> AddAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(string id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task<Result> ToggleStatus(string id);
    Task<Result> Unlock(string id);
    Task<Result<PatientProfileResponse>> GetProfileAsync(string patientId);
    Task<Result> UpdateProfileAsync(string patientId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string patientId, ChangePasswordRequest request);
}

