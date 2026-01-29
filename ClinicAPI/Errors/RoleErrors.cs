namespace ClinicAPI.Errors;

public class RoleErrors
{
      public static readonly Error RoleNotFound =
        new("Role.NotFound", "Role is not found", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedRole =
        new("Role.DuplicatedRole", "Role already exists", StatusCodes.Status409Conflict);

    public static readonly Error InvalidPermissions =
        new("Role.InvalidPermissions", "Permissions are not in the allowed permissions list", StatusCodes.Status400BadRequest);
}
