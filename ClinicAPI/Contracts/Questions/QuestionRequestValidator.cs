namespace ClinicAPI.Contracts.Questions;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(q => q.Content)
            .NotEmpty().WithMessage("{PropertyName} Can't be Empty")
            .Length(3,1000).WithMessage("{PropertyName} Should be between 3 and 1000 character length");

        RuleFor(q => q.Answers)
            .NotNull();

        RuleFor(q => q.Answers)
            .Must(a=>!a.Contains("")).WithMessage("Empty string answers are not allowed")
            .Must(a => a.Count >= 2).WithMessage("Answers can't be less than 2")
            .Must(a => a.Distinct().Count() == a.Count).WithMessage("Answers should be unique for every question")
            .When(a => a.Answers != null);
    }
}