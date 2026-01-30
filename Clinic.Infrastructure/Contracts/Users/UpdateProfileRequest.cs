namespace Clinic.Infrastructure.Contracts.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);
