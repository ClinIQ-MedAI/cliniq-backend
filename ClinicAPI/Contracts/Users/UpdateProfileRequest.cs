namespace ClinicAPI.Contracts.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);