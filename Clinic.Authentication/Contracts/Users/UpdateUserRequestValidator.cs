using FluentValidation;

namespace Clinic.Authentication.Contracts.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(u => u.FirstName)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(u => u.LastName)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(u => u.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(u => u.Roles)
            .NotNull()
            .NotEmpty();

        RuleFor(u => u.Roles)
            .Must(r => r.Distinct().Count() == r.Count)
            .WithMessage("Roles must be unique for every user")
            .When(r => r.Roles != null);
    }
}

