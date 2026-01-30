using FluentValidation;
using Clinic.Infrastructure.Abstractions.Consts;

namespace Clinic.Authentication.Contracts.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(r => r.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 chars and Contain: LowerCase, UpperCase, Number, and NonAlphanumeric");

        RuleFor(r => r.FirstName)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(r => r.LastName)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(r => r.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today.AddYears(-13))
            .WithMessage("User must be at least 13 years old");
    }
}
