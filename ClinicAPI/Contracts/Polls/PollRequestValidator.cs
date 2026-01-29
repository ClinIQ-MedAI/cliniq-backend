namespace ClinicAPI.Contracts.Polls;

public class PollRequestValidator : AbstractValidator<PollRequest>
{
    public PollRequestValidator() 
    {
        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Please Add a {PropertyName}.")
            .Length(3,100).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters, You Entered: '{PropertyValue}'.");
        
        RuleFor(p => p.Summary)
            .NotEmpty().WithMessage("Please Add a {PropertyName}.")
            .Length(3, 1500).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters, You Entered: '{PropertyValue}'.");
    
        RuleFor(p => p.StartsAt)
            .NotEmpty().WithMessage("Please Add a {PropertyName}.")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today)).WithMessage("{PropertyName} must be in the future or Today, You Entered: '{PropertyValue}'.");

        RuleFor(p => p.EndsAt)
            .NotEmpty().WithMessage("Please Add a {PropertyName}.");
        //.GreaterThan(p => p.StartsAt).WithMessage("{PropertyName} must be after the Start Date, You Entered: '{PropertyValue}'.");


        RuleFor(p => p)
            .Must(HasValidDates).WithMessage("End Date must be after or equal to Start Date.");




    }

    private bool HasValidDates(PollRequest pollRequest)
    {
        return pollRequest.EndsAt >= pollRequest.StartsAt;
    }


}
