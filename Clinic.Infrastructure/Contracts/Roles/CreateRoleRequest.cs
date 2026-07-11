namespace Clinic.Infrastructure.Contracts.Roles;

public record CreateRoleRequest(
    string Name,
    string[] Permissions
);
