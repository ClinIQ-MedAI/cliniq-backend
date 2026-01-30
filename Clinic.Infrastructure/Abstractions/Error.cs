namespace Clinic.Infrastructure.Abstractions;

public record Error(string Code, string Description, int? StatusCode)
{
    public static readonly Error None = new(string.Empty, string.Empty, null);

    public static Error Failure(string code, string description, int? statusCode = 400) => new(code, description, statusCode);
    public static Error NotFound(string code, string description) => new(code, description, 404);
    public static Error Conflict(string code, string description) => new(code, description, 409);
}
