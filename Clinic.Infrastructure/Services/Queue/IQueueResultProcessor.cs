using Clinic.Infrastructure.Services.Queue.Contracts;

namespace Clinic.Infrastructure.Services.Queue;

public interface IQueueResultProcessor
{
    Task ProcessJobResultAsync(ResultMessage result);
    Task ProcessChatReplyAsync(ChatReplyMessage reply);
}
