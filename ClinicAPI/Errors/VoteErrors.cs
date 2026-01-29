namespace ClinicAPI.Errors;

public static class VoteErrors
{
    public static readonly Error DuplicatedVote =
        new("Vote.Duplicated", "This user Has Voted on the same poll before", StatusCodes.Status409Conflict);

    public static readonly Error InvalidQuestions =
        new("Vote.InvalidQuestions", "Invalid Questions", StatusCodes.Status400BadRequest);

    public static readonly Error AnswersDontBelongToQuestion =
        new("Vote.AnswersDontBelongToQuestion", "Answers Don't Belong To it's Question", StatusCodes.Status400BadRequest);
}