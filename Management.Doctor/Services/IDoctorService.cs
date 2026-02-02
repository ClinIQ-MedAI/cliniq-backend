using Clinic.Infrastructure.Contracts.Doctors;

namespace Management.Doctor.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<DoctorResponse>> GetAsync(string id);
    Task<Result<DoctorResponse>> AddAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(string id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(string id, bool active);
    Task<Result> Unlock(string id);
    Task<Result> ApproveAsync(string id);
    Task<Result> RejectAsync(string id, string reason);
}
