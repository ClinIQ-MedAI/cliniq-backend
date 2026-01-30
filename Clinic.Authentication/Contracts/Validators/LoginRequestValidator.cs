using FluentValidation;

namespace Clinic.Authentication.Contracts.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .EmailAddress().WithMessage("{PropertyName} Must be Valid Email.");

        RuleFor(l => l.Password)
            .NotEmpty().When(l => string.IsNullOrEmpty(l.OtpCode))
            .WithMessage("{PropertyName} is required when OTP is not provided.");

        RuleFor(l => l.OtpCode)
            .NotEmpty().When(l => string.IsNullOrEmpty(l.Password))
            .WithMessage("{PropertyName} is required when Password is not provided.");
    }
}
