namespace Clinic.Infrastructure.Abstractions;

/// <summary>
/// Common interface for application user entities (Doctor, Patient).
/// Allows shared infrastructure services to work with any user type.
/// </summary>
public interface IApplicationUser
{
    string Id { get; }
    string? Email { get; }
    string FirstName { get; set; }
    string LastName { get; set; }
}
