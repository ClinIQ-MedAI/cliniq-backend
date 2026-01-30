using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinic.Authentication.Services;

/// <summary>
/// Unified registration service - creates ApplicationUser only.
/// Profiles are created via separate survey submissions.
/// </summary>
public class RegistrationService(UserManager<ApplicationUser> userManager) : IRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists is not null)
            return Result.Failure(UserErrors.EmailDuplicated);

        // Check phone uniqueness if provided
        if (!string.IsNullOrEmpty(request.Phone))
        {
            var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == request.Phone);
            if (phoneExists)
                return Result.Failure(new Error("User.PhoneDuplicated", "Phone number is already in use", StatusCodes.Status409Conflict));
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.Phone,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Succeed();
    }
}
