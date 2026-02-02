using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Clinic.Authentication.Contracts.Users;
using Microsoft.AspNetCore.Identity;

namespace Profile.Doctor.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var response = new UserProfileResponse(
            user!.Email!,
            user.UserName!,
            user.FirstName,
            user.LastName
        );

        return Result.Succeed(response);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _userManager.UpdateAsync(user);

        return Result.Succeed();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Succeed();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
