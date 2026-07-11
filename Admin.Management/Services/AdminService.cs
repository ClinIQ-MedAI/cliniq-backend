using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Admin.Management.Services;

public class AdminService(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender) : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task<Result<List<AdminResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .Where(u => u.DoctorProfile == null && u.PatientProfile == null)
            .ToListAsync(cancellationToken);

        var result = new List<AdminResponse>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();
            if (roles.Length == 0) continue;

            result.Add(MapToResponse(user, roles));
        }

        return Result.Succeed(result);
    }

    public async Task<Result<AdminResponse>> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return Result.Failure<AdminResponse>(Error.NotFound("User.NotFound", "User not found"));

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        if (roles.Length == 0)
            return Result.Failure<AdminResponse>(Error.NotFound("User.NotFound", "User not found"));

        return Result.Succeed(MapToResponse(user, roles));
    }

    public async Task<Result<AdminResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists is not null)
            return Result.Failure<AdminResponse>(Error.Conflict("User.DuplicateEmail", "Email already exists"));

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<AdminResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        foreach (var role in request.Roles)
            await _userManager.AddToRoleAsync(user, role);

        var placeHolders = new Dictionary<string, string>
        {
            { "{{name}}", $"{request.FirstName} {request.LastName}" },
            { "{{email}}", request.Email },
            { "{{password}}", request.Password }
        };

        var emailBody = EmailBodyBuilder.GenerateEmailBody("AdminCredentials", placeHolders);
        await _emailSender.SendEmailAsync(request.Email, "Clinic API: Your Admin Account", emailBody);

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        return Result.Succeed(MapToResponse(user, roles));
    }

    public async Task<Result> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Result.Succeed()
            : Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleDisableAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));

        user.IsDisabled = !user.IsDisabled;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Succeed()
            : Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
    }

    private static AdminResponse MapToResponse(ApplicationUser user, string[] roles) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email!,
        user.PhoneNumber,
        user.EmailConfirmed,
        user.PhoneNumberConfirmed,
        user.IsDisabled,
        roles
    );
}
