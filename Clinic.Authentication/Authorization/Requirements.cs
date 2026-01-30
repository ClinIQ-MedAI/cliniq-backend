using Microsoft.AspNetCore.Authorization;

namespace Clinic.Authentication.Authorization;

/// <summary>
/// Requirement for verification status check.
/// </summary>
public class VerificationRequirement : IAuthorizationRequirement
{
    public bool RequireEmailVerified { get; }
    public bool RequirePhoneVerified { get; }
    public bool RequireAny { get; }

    public VerificationRequirement(bool requireAny = true, bool requireEmailVerified = false, bool requirePhoneVerified = false)
    {
        RequireAny = requireAny;
        RequireEmailVerified = requireEmailVerified;
        RequirePhoneVerified = requirePhoneVerified;
    }
}

/// <summary>
/// Requirement for patient status check.
/// </summary>
public class PatientStatusRequirement : IAuthorizationRequirement
{
    public string[] AllowedStatuses { get; }

    public PatientStatusRequirement(params string[] allowedStatuses)
    {
        AllowedStatuses = allowedStatuses;
    }
}

/// <summary>
/// Requirement for doctor status check.
/// </summary>
public class DoctorStatusRequirement : IAuthorizationRequirement
{
    public string[] AllowedStatuses { get; }

    public DoctorStatusRequirement(params string[] allowedStatuses)
    {
        AllowedStatuses = allowedStatuses;
    }
}
