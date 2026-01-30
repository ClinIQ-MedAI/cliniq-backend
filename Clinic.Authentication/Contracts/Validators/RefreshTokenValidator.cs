using FluentValidation;

namespace Clinic.Authentication.Contracts.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(r => r.Token)
            .NotEmpty();

        RuleFor(r => r.RefreshToken)
            .NotEmpty();
    }
}
