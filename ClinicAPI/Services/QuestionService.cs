using Microsoft.Extensions.Caching.Hybrid;
using ClinicAPI.Contracts.Answers;
using ClinicAPI.Contracts.Common;
using ClinicAPI.Contracts.Questions;
using System.Linq.Dynamic.Core;

namespace ClinicAPI.Services;

public class QuestionService(
    ApplicationDbContext context,
    //ICacheService cacheService,
    HybridCache hybridCache,
    ILogger<QuestionService> logger) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;
    //private readonly ICacheService _cacheService = cacheService;
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<QuestionService> _logger = logger;

    private const string _cachePrefix = "availableQuestions";

    public async Task<Result<PaginatedList<QuestionResponse>>> GetAllAsync(int pollId, RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var pollExists = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);
        if (!pollExists) return Result.Failure<PaginatedList<QuestionResponse>>(PollErrors.PollNotFound);

        var query = _context.Questions
            .Where(q => q.PollId == pollId);

        if (!string.IsNullOrEmpty(filters.SearchValue))
            query = query.Where(q => q.Content.Contains(filters.SearchValue));

        if (!string.IsNullOrEmpty(filters.SortColumn))
            query = query.OrderBy($"{filters.SortColumn} {filters.SortDirection}");

        var source = query.Include(q => q.Answers)
                        .ProjectToType<QuestionResponse>()
                        .AsNoTracking();

        var questions = await PaginatedList<QuestionResponse>.CreateAsync(source, filters.PageNumber, filters.PageSize, cancellationToken);

        return Result.Succeed(questions);
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        var isValidPoll = await _context.Polls.AnyAsync( p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isValidPoll) return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var userVotedBefore = await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);
        if (userVotedBefore) return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DuplicatedVote);

        //Caching

        var cacheKey = $"{_cachePrefix}-{pollId}";

        //Caching using Distributed Cache

        //var cachedQuestions = await _cacheService.GetAsync<IEnumerable<QuestionResponse>>(cacheKey, cancellationToken);

        //IEnumerable<QuestionResponse> questions = [];

        //if(cachedQuestions is null)
        //{
        //    _logger.LogInformation("Select questions from database");
        //    questions = await _context.Questions
        //        .Where(q => q.PollId == pollId && q.IsActive)
        //        .Include(q => q.Answers)
        //        .Select(q => new QuestionResponse(
        //            q.Id,
        //            q.Content,
        //            q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
        //        ))
        //        .AsNoTracking()
        //        .ToListAsync(cancellationToken);

        //    await _cacheService.SetAsync(cacheKey, questions, cancellationToken);
        //}
        //else
        //{
        //    _logger.LogInformation("Get questions from cache");
        //    questions = cachedQuestions;
        //}

        //Caching using Hybrid Cache

        var questions = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>(
            cacheKey,
            async cacheEntry => await _context.Questions
            .Where(q => q.PollId == pollId && q.IsActive)
            .Include(q => q.Answers)
            .Select(q => new QuestionResponse(
                q.Id,
                q.Content,
                q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken),
            cancellationToken: cancellationToken
        );

        return Result.Succeed(questions);
    }

    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .Where(q => q.PollId == pollId && q.Id == id)
            .Include(q => q.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null) return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        return Result.Succeed(question);
    }

    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var pollExists = await _context.Polls.AnyAsync(q => q.Id == pollId, cancellationToken: cancellationToken);
        if (!pollExists) return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questionIsDuplicated = await _context.Questions.AnyAsync(q => q.Content == request.Content && q.PollId == pollId, cancellationToken: cancellationToken);
        if (questionIsDuplicated) return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        var question = request.Adapt<Question>();
        question.PollId = pollId;

        await _context.AddAsync(question, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);

        return Result.Succeed(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .Include(q => q.Answers)
            .SingleOrDefaultAsync(q => q.PollId == pollId && q.Id == id, cancellationToken);
        if (question is null) return Result.Failure(QuestionErrors.QuestionNotFound);

        var questionIsDuplicated = await _context.Questions
            .AnyAsync(q => q.Id != id
                && q.PollId == pollId 
                && q.Content == request.Content,
                cancellationToken: cancellationToken
            );
        if (questionIsDuplicated) return Result.Failure(QuestionErrors.DuplicatedQuestionContent);

        question.Content = request.Content; 
        foreach (var answer in question.Answers)  answer.IsActive = false;    // Deactivate old Answers 

        foreach (var answer in request.Answers) 
        {
            var existingAnswer = question.Answers.SingleOrDefault(a => a.Content == answer);

            if(existingAnswer is null)
                question.Answers.Add(new Answer { Content = answer });   // if answer doesn't exist in current answers, add it
            else existingAnswer.IsActive = true;     // if answer exists, activate it
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);

        return Result.Succeed();
    }

    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions.SingleOrDefaultAsync(q => q.Id == id && q.PollId == pollId, cancellationToken);
        if (question is null) return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);

        return Result.Succeed();
    }
}
