namespace Clinic.Infrastructure.Contracts.Admins;

public record AdminResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool IsDisabled,
    string[] Roles
);
