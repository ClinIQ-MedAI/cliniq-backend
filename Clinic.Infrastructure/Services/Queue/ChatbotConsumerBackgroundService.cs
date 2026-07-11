using System.Text.Json;
using System.Text.Json.Serialization;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Services.Queue;

public class ChatbotConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatbotConsumerBackgroundService> _logger;
    private readonly QueueSettings _settings;
    private readonly IHubContext<ChatbotHub> _hubContext;

    public ChatbotConsumerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ChatbotConsumerBackgroundService> logger,
        IOptions<QueueSettings> settings,
        IHubContext<ChatbotHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var backend = _settings.QueueBackend?.ToLowerInvariant();
        if (backend != "redis")
        {
            _logger.LogInformation("Chatbot consumer background service is disabled (QUEUE_BACKEND is '{Backend}').", _settings.QueueBackend);
            return;
        }

        _logger.LogInformation("Starting Chatbot consumer background service for Redis Streams.");
        await RunRedisConsumerAsync(stoppingToken);
    }

    private async Task RunRedisConsumerAsync(CancellationToken stoppingToken)
    {
        IConnectionMultiplexer redis;
        try
        {
            redis = _serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve IConnectionMultiplexer. Cannot start Chatbot Redis Stream Consumer.");
            return;
        }

        var db = redis.GetDatabase();
        var stream = _settings.ChatResultChannel;
        var group = _settings.ChatGroup;
        var consumerName = $"chatbot-api-{Guid.NewGuid():N}";

        _logger.LogInformation("Initializing Chatbot Redis Stream Consumer. Stream: {Stream}, Group: {Group}, Consumer: {Consumer}", stream, group, consumerName);

        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, group, "0-0", true);
            _logger.LogInformation("Created consumer group {Group} on stream {Stream}.", group, stream);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            _logger.LogInformation("Consumer group {Group} already exists on stream {Stream}.", group, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Chatbot Redis Stream consumer group.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    stream,
                    group,
                    consumerName,
                    position: ">",
                    count: 10
                );

                if (entries == null || entries.Length == 0)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    var data = entry.Values.FirstOrDefault(v => v.Name == "data");
                    if (!data.Value.HasValue) continue;

                    string json = data.Value.ToString();
                    _logger.LogInformation("Received chatbot reply from Redis Stream: {Message}", json);

                    try
                    {
                        var reply = JsonSerializer.Deserialize<ChatReplyPayload>(json);
                        if (reply != null)
                        {
                            await ProcessChatReplyAsync(reply);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing chatbot reply.");
                    }

                    await db.StreamAcknowledgeAsync(stream, group, entry.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Chatbot Redis Stream read loop.");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessChatReplyAsync(ChatReplyPayload reply)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbMessage = await context.AIChatMessages.FirstOrDefaultAsync(m => m.ChatId == reply.ChatId);
        if (dbMessage != null)
        {
            var status = reply.Status;
            if (!string.IsNullOrEmpty(status))
            {
                status = char.ToUpper(status[0]) + status.Substring(1).ToLowerInvariant();
            }

            dbMessage.Status = status;
            dbMessage.Reply = reply.Reply;
            dbMessage.QueryType = reply.QueryType;
            dbMessage.ShowUpload = reply.ShowUpload;
            dbMessage.Error = reply.Error;
            dbMessage.Worker = reply.Worker;
            dbMessage.DurationMs = reply.DurationMs;

            if (DateTime.TryParse(reply.FinishedAt, out var finishedAt))
            {
                dbMessage.FinishedAt = finishedAt;
            }
            else
            {
                dbMessage.FinishedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Chat message {ChatId} updated successfully to status {Status}", reply.ChatId, dbMessage.Status);

            // Push to patient via SignalR
            await _hubContext.Clients.Group($"patient_{reply.PatientId}").SendAsync("ReceiveChatbotReply", reply);
        }
        else
        {
            _logger.LogWarning("Received reply for unknown Chat ID: {ChatId}", reply.ChatId);
        }
    }

    private class ChatReplyPayload
    {
        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("reply")]
        public string? Reply { get; set; }

        [JsonPropertyName("query_type")]
        public string? QueryType { get; set; }

        [JsonPropertyName("show_upload")]
        public bool ShowUpload { get; set; }

        [JsonPropertyName("patient_id")]
        public string PatientId { get; set; } = string.Empty;

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("worker")]
        public string? Worker { get; set; }

        [JsonPropertyName("duration_ms")]
        public double? DurationMs { get; set; }

        [JsonPropertyName("finished_at")]
        public string? FinishedAt { get; set; }
    }
}
