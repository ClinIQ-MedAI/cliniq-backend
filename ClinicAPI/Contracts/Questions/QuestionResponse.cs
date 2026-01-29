using ClinicAPI.Contracts.Answers;

namespace ClinicAPI.Contracts.Questions;

public record QuestionResponse(
    int Id,
    string Content,
    IEnumerable<AnswerResponse> Answers
);