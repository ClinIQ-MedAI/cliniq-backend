namespace ClinicAPI.Contracts.Authentication;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator() 
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .EmailAddress().WithMessage("{PropertyName} Must be Valid Email.");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.");
    }
}
