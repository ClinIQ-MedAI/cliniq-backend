namespace Clinic.Infrastructure.Contracts.Doctors;

public record CreateDoctorRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);
