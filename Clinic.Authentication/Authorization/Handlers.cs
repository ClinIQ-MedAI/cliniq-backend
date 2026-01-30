using Microsoft.AspNetCore.Authorization;

namespace Clinic.Authentication.Authorization;

/// <summary>
/// Handler for verification requirement.
/// </summary>
public class VerificationRequirementHandler : AuthorizationHandler<VerificationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VerificationRequirement requirement)
    {
        var emailConfirmed = context.User.FindFirst("email_confirmed")?.Value == "true";
        var phoneConfirmed = context.User.FindFirst("phone_number_confirmed")?.Value == "true";

        if (requirement.RequireAny)
        {
            if (emailConfirmed || phoneConfirmed)
                context.Succeed(requirement);
        }
        else
        {
            if ((requirement.RequireEmailConfirmed && emailConfirmed) ||
                (requirement.RequirePhoneNumberConfirmed && phoneConfirmed) ||
                (!requirement.RequireEmailConfirmed && !requirement.RequirePhoneNumberConfirmed))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for patient status requirement.
/// </summary>
public class PatientStatusRequirementHandler : AuthorizationHandler<PatientStatusRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PatientStatusRequirement requirement)
    {
        var patientStatus = context.User.FindFirst("patient_status")?.Value;

        if (patientStatus != null && requirement.AllowedStatuses.Contains(patientStatus))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for doctor status requirement.
/// </summary>
public class DoctorStatusRequirementHandler : AuthorizationHandler<DoctorStatusRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DoctorStatusRequirement requirement)
    {
        var doctorStatus = context.User.FindFirst("doctor_status")?.Value;

        if (doctorStatus != null && requirement.AllowedStatuses.Contains(doctorStatus))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
