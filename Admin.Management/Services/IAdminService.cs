namespace Admin.Management.Services;

public interface IAdminService
{
    Task<Result<List<AdminResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AdminResponse>> GetByIdAsync(string id);
    Task<Result<AdminResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string id);
    Task<Result> ToggleDisableAsync(string id);
}
