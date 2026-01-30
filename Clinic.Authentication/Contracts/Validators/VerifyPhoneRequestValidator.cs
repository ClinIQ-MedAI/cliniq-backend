using FluentValidation;

namespace Clinic.Authentication.Contracts.Validators;

public class VerifyPhoneRequestValidator : AbstractValidator<VerifyPhoneRequest>
{
    public VerifyPhoneRequestValidator()
    {
        RuleFor(r => r.Phone)
            .NotEmpty();

        RuleFor(r => r.Code)
            .NotEmpty();
    }
}
