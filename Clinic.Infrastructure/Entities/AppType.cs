namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Represents the type of application/client the user is logging into.
/// Used to validate that the user has the appropriate profile.
/// </summary>
public enum AppType
{
    Doctor,
    Patient,
    Dashboard
}
