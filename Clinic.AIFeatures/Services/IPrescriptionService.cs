using Clinic.AIFeatures.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.AIFeatures.Services;

public interface IPrescriptionService
{
    Task<Result<ParsedPrescriptionResponse>> UploadPrescriptionAsync(UploadPrescriptionRequest request, CancellationToken cancellationToken = default);
    Task<Result<ParsedPrescriptionResponse>> GetPrescriptionAsync(int prescriptionId);
    Task<IEnumerable<ParsedPrescriptionResponse>> GetPatientPrescriptionsAsync(string patientId);
    Task<Result> ConfirmPrescriptionAsync(int prescriptionId, ConfirmPrescriptionRequest request);
}
