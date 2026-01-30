namespace Clinic.Infrastructure.Contracts.Doctors;

public record UpdateDoctorRequest(
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);
