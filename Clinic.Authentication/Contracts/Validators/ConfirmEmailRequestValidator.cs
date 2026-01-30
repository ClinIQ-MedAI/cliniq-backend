using FluentValidation;

namespace Clinic.Authentication.Contracts.Validators;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(r => r.UserId)
            .NotEmpty();

        RuleFor(r => r.Code)
            .NotEmpty();
    }
}
