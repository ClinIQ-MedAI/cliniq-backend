using ClinicAPI.Abstractions.Consts;

namespace ClinicAPI.Contracts.Authentication;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(r => r.Code)
            .NotEmpty();

        RuleFor(r => r.NewPassword)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 chars and Contain: LowerCase, UpperCase, Number, and NonAlphanumeric");
    }
}
