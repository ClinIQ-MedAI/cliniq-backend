namespace Clinic.Infrastructure.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = 
        new("User.InvalidCredentials", "Incorrect Email or Password", StatusCodes.Status401Unauthorized);

    public static readonly Error UserNotFound =
        new("User.NotFound", "The user with the specified ID was not found.", StatusCodes.Status404NotFound);

    public static readonly Error LockedOut =
        new("User.LockedOut", "Locked Out, try again later", StatusCodes.Status401Unauthorized);

    public static readonly Error DisabledUser =
        new("User.DisabledUser", "Disabled user, please contact your adminstrator", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidJwtToken = 
        new("User.InvalidJwtToken", "Jwt token is not valid", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidRefreshToken = 
        new("User.InvalidRefreshToken", "Refresh token is not valid", StatusCodes.Status401Unauthorized);

    public static readonly Error EmailDuplicated =
        new("User.EmailDuplicated", "Another user With the same Email exists", StatusCodes.Status409Conflict);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidCode =
        new("User.InvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicatedEmailConfirmation =
        new("User.DuplicatedEmailConfirmation", "Email already confirmed", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRoles =
        new("User.InvalidRoles", "Roles are not in the allowed Roles list", StatusCodes.Status400BadRequest);
}
