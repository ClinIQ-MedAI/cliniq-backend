using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Authentication.Contracts;

/// <summary>
/// Result of an authentication attempt.
/// </summary>
public record AuthResult(
    bool IsSuccess,
    string? Token,
    string? RefreshToken,
    DateTime? ExpiresAt,
    string? Error,
    PatientStatus? PatientStatus,
    DoctorStatus? DoctorStatus
)
{
    public static AuthResult Success(
        string token,
        string refreshToken,
        DateTime expiresAt,
        PatientStatus patientStatus,
        DoctorStatus doctorStatus)
        => new(true, token, refreshToken, expiresAt, null, patientStatus, doctorStatus);

    public static AuthResult Failure(string error)
        => new(false, null, null, null, error, null, null);
}
