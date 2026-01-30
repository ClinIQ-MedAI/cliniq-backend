using FluentValidation;
using Clinic.Authentication.Strategies; // For RegexPatterns if needed, or Shared

// Assuming RegexPatterns is in Clinic.Authentication or Shared.
// If it was in Infrastructure, we need to know where it is.
// Actually permissions.cs was moved, but RegexPatterns?
// Let's check where RegexPatterns is.
// For now, I'll copy the validator logic.

using Clinic.Authentication.Contracts.Users;
using Clinic.Infrastructure.Abstractions.Consts; // RegexPatterns is likely here

namespace Clinic.Authentication.Contracts.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .NotEqual(x => x.CurrentPassword)
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must comprise 8 chars, one LowerCase, one UpperCase, one Number, and one non-alphanumeric");
    }
}
