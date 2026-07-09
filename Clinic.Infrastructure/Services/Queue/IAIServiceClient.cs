using Clinic.Infrastructure.Services.Queue.Contracts;

namespace Clinic.Infrastructure.Services.Queue;

public interface IAIServiceClient
{
    Task<ResultMessage> SendPredictRequestAsync(string modality, JobMessage request, CancellationToken cancellationToken = default);
}
