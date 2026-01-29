using ClinicAPI.Contracts.Roles;

namespace ClinicAPI.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? IncludeDisabled = false, CancellationToken cancellationToken = default)=>
        await _roleManager.Roles
            .Where(role => !role.IsDefault && (!role.IsDeleted || IncludeDisabled.HasValue && IncludeDisabled.Value))
            .ProjectToType<RoleResponse>()
            .ToListAsync(cancellationToken);

    public async Task<Result<RoleDetailsResponse>> GetAsync(string id)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailsResponse>(RoleErrors.RoleNotFound);

        var permissions = await _roleManager.GetClaimsAsync(role);

        var response = new RoleDetailsResponse(role.Id, role.Name!, role.IsDeleted, permissions.Select(p => p.Value));

        return Result.Succeed(response);
    }

    public async Task<Result<RoleDetailsResponse>> AddAsync(RoleRequest request)
    {
        var roleExists = await _roleManager.RoleExistsAsync(request.Name);
        if (roleExists)
            return Result.Failure<RoleDetailsResponse>(RoleErrors.DuplicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();
        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailsResponse>(RoleErrors.InvalidPermissions);

        var role = new ApplicationRole
        {
            Name = request.Name,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            var permissions = request.Permissions
                .Select(p => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = p,
                    RoleId = role.Id
                });

            await _context.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();

            var response = new RoleDetailsResponse(role.Id, role.Name, role.IsDeleted, request.Permissions);
            
            return Result.Succeed(response);
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailsResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, RoleRequest request)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure(RoleErrors.RoleNotFound);

        var roleExists = await _roleManager.Roles.AnyAsync(r => r.Name == request.Name && r.Id != id);
        if (roleExists)
            return Result.Failure(RoleErrors.DuplicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();
        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure(RoleErrors.InvalidPermissions);

        role.Name = request.Name;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            var currentPermissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == id && rc.ClaimType == Permissions.Type)
                .Select(rc => rc.ClaimValue)
                .ToListAsync();

            var removedPermissions = currentPermissions.Except(request.Permissions);

            await _context.RoleClaims
                .Where(rc => rc.RoleId == id && removedPermissions.Contains(rc.ClaimValue))
                .ExecuteDeleteAsync();

            var AddedPermissions = request.Permissions.Except(currentPermissions)
                .Select(p => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = p,
                    RoleId = role.Id
                });

            await _context.AddRangeAsync(AddedPermissions);
            await _context.SaveChangesAsync();

            return Result.Succeed();
        }

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleStatusAsync(string id)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure(RoleErrors.RoleNotFound);

        role.IsDeleted = !role.IsDeleted;

        await _roleManager.UpdateAsync(role);

        return Result.Succeed();
    }
}