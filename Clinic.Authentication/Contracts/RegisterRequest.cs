using Clinic.Infrastructure.Entities.Enums;

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
    DateOnly DateOfBirth,
    Gender Gender
);
