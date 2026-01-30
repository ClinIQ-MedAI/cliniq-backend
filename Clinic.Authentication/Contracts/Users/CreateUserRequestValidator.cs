using FluentValidation;
using Clinic.Infrastructure.Abstractions.Consts;

namespace Clinic.Authentication.Contracts.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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

        RuleFor(u => u.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 chars and Contain: LowerCase, UpperCase, Number, and NonAlphanumeric");

        RuleFor(u => u.Roles)
            .NotNull()
            .NotEmpty();

        RuleFor(u => u.Roles)
            .Must(r => r.Distinct().Count() == r.Count)
            .WithMessage("Roles must be unique for every user")
            .When(r => r.Roles != null);
    }
}

