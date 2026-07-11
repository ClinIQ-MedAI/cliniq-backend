namespace Clinic.Infrastructure.Contracts.Roles;

public record UpdateRoleRequest(
    string? Name,
    string[]? Permissions
);
