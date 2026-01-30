namespace Clinic.Infrastructure.Contracts.Users;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(r => r.LastName)
            .NotEmpty()
            .Length(3, 100);
    }
}

