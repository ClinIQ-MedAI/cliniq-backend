using Clinic.Authentication.Contracts.Roles;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.Authentication.Services;

public interface IRoleService
{
    Task<Result<List<RoleResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleResponse>> GetByIdAsync(string id);
    Task<Result<RoleResponse>> CreateAsync(CreateRoleRequest request);
    Task<Result<RoleResponse>> UpdateAsync(string id, UpdateRoleRequest request);
    Task<Result> DeleteAsync(string id);
}
