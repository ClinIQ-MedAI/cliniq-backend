namespace ClinicAPI.Contracts.Results;

public record VotesPerAnswerResponse(
    string Answer,
    int Count
);