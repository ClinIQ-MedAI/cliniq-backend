using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace Clinic.Authentication.Strategies;

/// <summary>
/// Login strategy using password.
/// </summary>
public class PasswordLoginStrategy(UserManager<ApplicationUser> userManager) : ILoginStrategy
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public bool CanHandle(LoginRequest request)
    {
        return !string.IsNullOrEmpty(request.Password);
    }

    public async Task<bool> ValidateAsync(ApplicationUser user, LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Password))
            return false;

        return await _userManager.CheckPasswordAsync(user, request.Password);
    }
}
