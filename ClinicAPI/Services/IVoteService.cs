using ClinicAPI.Contracts.Votes;

namespace ClinicAPI.Services;

public interface IVoteService
{
    Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default);
}
