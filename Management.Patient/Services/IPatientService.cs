using Clinic.Infrastructure.Contracts.Patients;

namespace Management.Patient.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PatientResponse>> GetAsync(string id);
    Task<Result<PatientResponse>> AddAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(string id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(string id, bool active);
    Task<Result> Unlock(string id);
}
