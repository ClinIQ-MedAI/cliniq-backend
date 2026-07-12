using System.Text.Json;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Clinic.Infrastructure.Services.Queue;

public class ChatbotConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatbotConsumerBackgroundService> _logger;
    private readonly QueueSettings _settings;
    private readonly IQueueResultProcessor _resultProcessor;

    public ChatbotConsumerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ChatbotConsumerBackgroundService> logger,
        IOptions<QueueSettings> settings,
        IQueueResultProcessor resultProcessor)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
        _resultProcessor = resultProcessor;
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
                        var reply = JsonSerializer.Deserialize<ChatReplyMessage>(json);
                        if (reply != null)
                        {
                            await _resultProcessor.ProcessChatReplyAsync(reply);
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
}
