namespace Clinic.Infrastructure.Contracts.Roles;

public record RoleResponse(
    string Id,
    string Name,
    bool IsDefault,
    bool IsDeleted,
    string[] Permissions
);
