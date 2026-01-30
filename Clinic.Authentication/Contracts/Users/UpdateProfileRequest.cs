namespace Clinic.Authentication.Contracts.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);
