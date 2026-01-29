using Microsoft.EntityFrameworkCore;
using ClinicAPI.Contracts.Questions;
using ClinicAPI.Contracts.Votes;

namespace ClinicAPI.Services;

public class VoteService(ApplicationDbContext context) : IVoteService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        var isValidPoll = await _context.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isValidPoll) return Result.Failure(PollErrors.PollNotFound);

        var userVotedBefore = await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);
        if (userVotedBefore) return Result.Failure(VoteErrors.DuplicatedVote);

        var availableQuestions = await _context.Questions
            .Where(q => q.PollId == pollId && q.IsActive)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        if (!request.Answers.Select(a => a.QuestionId).ToList().SequenceEqual(availableQuestions))
            return Result.Failure(VoteErrors.InvalidQuestions);

        //if (request.Answers.Any(ra => _context.Answers.Any(ca => ca.Id == ra.AnswerId && ca.QuestionId != ra.QuestionId)))
        //    return Result.Failure(VoteErrors.AnswersDontBelongToQuestion);

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            VoteAnswers = [.. request.Answers.Adapt<IEnumerable<VoteAnswer>>()]
        };

        await _context.AddAsync(vote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Succeed();
    }
}
