namespace ClinicAPI.Contracts.Votes;

public class VoteAnswerRequestValidator : AbstractValidator<VoteAnswerRequest>
{
    public VoteAnswerRequestValidator()
    {
        RuleFor(va => va.QuestionId)
            .GreaterThanOrEqualTo(1);
        
        RuleFor(va => va.AnswerId)
            .GreaterThanOrEqualTo(1);
    }
}
