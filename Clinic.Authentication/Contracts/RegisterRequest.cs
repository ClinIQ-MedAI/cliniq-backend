namespace Clinic.Authentication.Contracts;

/// <summary>
/// Request model for user registration.
/// </summary>
public record RegisterRequest(
    string Email,
    string? Phone,
    string FirstName,
    string LastName,
    string Password,
    DateTime DateOfBirth,
    string Gender  // "Male", "Female", "Other"
);
