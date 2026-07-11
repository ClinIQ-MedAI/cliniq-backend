namespace Clinic.Infrastructure.Contracts.Admins;

public record CreateAdminRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IList<string> Roles
);
