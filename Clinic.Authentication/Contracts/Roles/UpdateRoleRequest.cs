namespace Clinic.Authentication.Contracts.Roles;

public record UpdateRoleRequest(
    string? Name,
    string[]? Permissions
);
