using Clinic.Infrastructure.Entities;

namespace Clinic.Infrastructure.Services.Queue;

public interface IQueueService
{
    bool IsEnabled { get; }
    Task PublishJobAsync(AIJob job);
    Task PublishChatAsync(string chatId, string patientId, string message, string languagePreference);
}
