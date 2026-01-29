namespace ClinicAPI.Contracts.Authentication;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(l => l.Token)
            .NotEmpty().WithMessage("{PropertyName} is required.");

        RuleFor(l => l.RefreshToken)
            .NotEmpty().WithMessage("{PropertyName} is required.");
    }
}
