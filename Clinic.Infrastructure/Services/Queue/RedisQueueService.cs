using System.Text.Json;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Clinic.Infrastructure.Services.Queue;

public class RedisQueueService : IQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly QueueSettings _settings;

    public RedisQueueService(IConnectionMultiplexer redis, IOptions<QueueSettings> settings)
    {
        _redis = redis;
        _settings = settings.Value;
    }

    public bool IsEnabled => true;

    public async Task PublishJobAsync(AIJob job)
    {
        var db = _redis.GetDatabase();
        var prefix = _settings.QueuePrefix;
        
        // Construct stream name like cliniq:jobs:bone
        var streamName = $"{prefix}{(prefix.EndsWith(":") ? "" : ":")}jobs:{job.Modality.ToString().ToLowerInvariant()}";

        var jobMessage = new JobMessage
        {
            JobId = job.Id,
            Modality = job.Modality.ToString().ToLowerInvariant(),
            ImageBase64 = job.ImageBase64,
            ImageUrl = job.ImageUrl,
            PatientId = job.PatientId,
            Options = job.OptionsJson != null ? JsonSerializer.Deserialize<object>(job.OptionsJson) : null,
            ReplyTo = job.ReplyTo,
            EnqueuedAt = job.CreatedAt.ToString("o")
        };

        var json = JsonSerializer.Serialize(jobMessage);
        
        // Add to Redis Stream
        await db.StreamAddAsync(streamName, new NameValueEntry[] { new("data", json) });
    }

    public async Task PublishChatAsync(string chatId, string patientId, string message, string languagePreference)
    {
        var db = _redis.GetDatabase();
        var payload = new
        {
            chat_id = chatId,
            message = message,
            patient_id = patientId,
            language_preference = languagePreference,
            enqueued_at = DateTime.UtcNow.ToString("o")
        };

        var json = JsonSerializer.Serialize(payload);
        await db.StreamAddAsync(_settings.ChatRequestChannel, new NameValueEntry[] { new("data", json) });
    }
}
