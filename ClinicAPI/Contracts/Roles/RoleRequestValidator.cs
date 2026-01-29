namespace ClinicAPI.Contracts.Roles;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .Length(3, 200);

        RuleFor(r => r.Permissions)
            .NotEmpty()
            .NotNull();

        RuleFor(r => r.Permissions)
            .Must(p => !p.Contains("")).WithMessage("Empty string permissions are not allowed")
            .Must(p => p.Distinct().Count() == p.Count).WithMessage("Permissions must be unique.")
            .When(p => p is not null);
    }
}
