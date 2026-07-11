namespace Clinic.Infrastructure.Contracts.Users;

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
