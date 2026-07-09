using Clinic.Infrastructure.Entities;

namespace Clinic.Infrastructure.Services.Queue;

public class NullQueueService : IQueueService
{
    public bool IsEnabled => false;

    public Task PublishJobAsync(AIJob job)
    {
        throw new InvalidOperationException("Queue is disabled. Please configure QueueSettings:QueueBackend to 'redis' to use asynchronous queues.");
    }
}
