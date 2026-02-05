using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Authentication.Contracts;

/// <summary>
/// Response DTO for successful authentication.
/// </summary>
public record AuthTokenResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    PatientStatus PatientStatus,
    DoctorStatus DoctorStatus
);
