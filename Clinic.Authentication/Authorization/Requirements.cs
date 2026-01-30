using Microsoft.AspNetCore.Authorization;

namespace Clinic.Authentication.Authorization;

/// <summary>
/// Requirement for verification status check.
/// </summary>
public class VerificationRequirement : IAuthorizationRequirement
{
    public bool RequireEmailConfirmed { get; }
    public bool RequirePhoneNumberConfirmed { get; }
    public bool RequireAny { get; }

    public VerificationRequirement(bool requireAny = true, bool requireEmailConfirmed = false, bool requirePhoneNumberConfirmed = false)
    {
        RequireAny = requireAny;
        RequireEmailConfirmed = requireEmailConfirmed;
        RequirePhoneNumberConfirmed = requirePhoneNumberConfirmed;
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
