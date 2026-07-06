namespace Clinic.Authentication.Contracts.Roles;

public record CreateRoleRequest(
    string Name,
    string[] Permissions
);
