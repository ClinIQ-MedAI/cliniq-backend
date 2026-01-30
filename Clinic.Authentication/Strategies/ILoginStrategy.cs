using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;

namespace Clinic.Authentication.Strategies;

/// <summary>
/// Interface for login strategies (password, OTP, etc.)
/// </summary>
public interface ILoginStrategy
{
    /// <summary>
    /// Determines if this strategy can handle the given request.
    /// </summary>
    bool CanHandle(LoginRequest request);

    /// <summary>
    /// Validates the user credentials according to this strategy.
    /// </summary>
    Task<bool> ValidateAsync(ApplicationUser user, LoginRequest request, CancellationToken cancellationToken = default);
}
