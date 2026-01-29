namespace ClinicAPI.Contracts.Votes;

public class VoteRequestValidator : AbstractValidator<VoteRequest>
{
	public VoteRequestValidator()
	{
		RuleFor(v => v.Answers)
			.NotEmpty();

		RuleForEach(v => v.Answers)
			.SetInheritanceValidator(v => v.Add(new VoteAnswerRequestValidator()));
	}
}