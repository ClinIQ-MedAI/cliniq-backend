namespace Clinic.Infrastructure.Contracts.Doctors;

public record DoctorResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsDisabled,
    IEnumerable<string> Roles
);
