namespace ClinicAPI.Contracts.Results;

public record QuestionAnswerResponse(
    string Question,
    string Answer
);