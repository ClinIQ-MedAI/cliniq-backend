using ClinicAPI.Abstractions.Consts;

namespace ClinicAPI.Contracts.Users;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(r => r.CurrentPassword)
            .NotEmpty();

        RuleFor(r => r.NewPassword)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 chars and Contain: LowerCase, UpperCase, Number, and NonAlphanumeric")
            .NotEqual(r => r.CurrentPassword)
            .WithMessage("New Password cannot be the same as Current Password");
    }
}