using Clinic.AIFeatures.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.AIFeatures.Services;

public interface IScanService
{
    Task<Result<ScanResponse>> UploadScanAsync(UploadScanRequest request, CancellationToken cancellationToken = default);
    Task<Result<ScanResponse>> GetScanAsync(int scanId);
    Task<IEnumerable<ScanResponse>> GetPatientScansAsync(string patientId);
    Task<Result> ReviewScanAsync(int scanId, ReviewScanRequest request);
    Task<Result> ConfirmScanAsync(int scanId, ConfirmScanRequest request);
}
