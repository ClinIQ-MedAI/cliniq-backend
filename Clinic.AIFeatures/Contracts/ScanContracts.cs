namespace Clinic.AIFeatures.Contracts;

public record UploadScanRequest(
    string Modality,
    string? ImageBase64,
    string? ImageUrl,
    string PatientId,
    object? Options
);

public record ScanResponse(
    int Id,
    string PatientId,
    string PatientName,
    string Modality,
    string? ScanUrl,
    string? ScanBase64,
    string? AIJobId,
    string? AIJobStatus,
    object? AIAnalysisResult,
    string? DoctorId,
    string? DoctorName,
    string? DoctorNotes,
    DateTime? DoctorReviewDate,
    bool IsReviewed,
    DateTime CreatedAt
);

public record ReviewScanRequest(
    string DoctorId,
    string DoctorNotes
);
