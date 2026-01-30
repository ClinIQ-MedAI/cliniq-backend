using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.Authentication.Services;

/// <summary>
/// Interface for unified user registration.
/// Creates ApplicationUser only - profiles are created via surveys.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registers a new user with basic information.
    /// Does NOT create any profile (Doctor/Patient).
    /// </summary>
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
