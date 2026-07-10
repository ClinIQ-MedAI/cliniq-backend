namespace Clinic.AIFeatures.Contracts;

public record UploadPrescriptionRequest(
    string? ImageBase64,
    string? ImageUrl,
    string PatientId
);

public record ParsedPrescriptionResponse(
    int Id,
    string PatientId,
    string PatientName,
    string? AIJobId,
    string? AIJobStatus,
    string? PrescriptionImageUrl,
    string? PrescriptionImageBase64,
    object? RawParsedText,
    object? Medications,
    string? DoctorId,
    string? DoctorName,
    string? DoctorNotes,
    DateTime CreatedAt
);

public record ConfirmPrescriptionRequest(
    string DoctorId,
    object Medications,
    string? DoctorNotes
);
