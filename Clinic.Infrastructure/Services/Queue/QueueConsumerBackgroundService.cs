using System.Text.Json;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Services.Queue;

public class QueueConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueueConsumerBackgroundService> _logger;
    private readonly QueueSettings _settings;
    private readonly IQueueResultProcessor _resultProcessor;

    public QueueConsumerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<QueueConsumerBackgroundService> logger,
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
            _logger.LogInformation("Queue consumer background service is disabled (QUEUE_BACKEND is '{Backend}').", _settings.QueueBackend);
            return;
        }

        _logger.LogInformation("Starting Queue consumer background service for Redis Streams.");
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
            _logger.LogError(ex, "Failed to resolve IConnectionMultiplexer. Cannot start Redis Stream Consumer.");
            return;
        }

        var db = redis.GetDatabase();
        var stream = _settings.QueueResultChannel;
        var group = _settings.QueueGroup;
        var consumerName = $"api-{Guid.NewGuid():N}";

        _logger.LogInformation("Initializing Redis Stream Consumer. Stream: {Stream}, Group: {Group}, Consumer: {Consumer}", stream, group, consumerName);

        try
        {
            // Create the consumer group on the results stream (createStream = true)
            await db.StreamCreateConsumerGroupAsync(stream, group, "0-0", true);
            _logger.LogInformation("Created consumer group {Group} on stream {Stream}.", group, stream);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            _logger.LogInformation("Consumer group {Group} already exists on stream {Stream}.", group, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Redis Stream consumer group.");
        }

        int currentDelayMs = 1000;
        const int minDelayMs = 1000;
        const int maxDelayMs = 10000; // 10 seconds max idle delay

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Read new messages that have never been delivered to other consumers (">")
                var entries = await db.StreamReadGroupAsync(
                    stream, 
                    group, 
                    consumerName, 
                    position: ">", 
                    count: 10
                );

                if (entries == null || entries.Length == 0)
                {
                    await Task.Delay(currentDelayMs, stoppingToken);
                    currentDelayMs = Math.Min(currentDelayMs * 2, maxDelayMs);
                    continue;
                }

                // Reset delay on processing message
                currentDelayMs = minDelayMs;

                foreach (var entry in entries)
                {
                    var data = entry.Values.FirstOrDefault(v => v.Name == "data");
                    if (!data.Value.HasValue) continue;

                    string json = data.Value.ToString();
                    _logger.LogInformation("Received message from Redis Stream: {Message}", json);

                    try
                    {
                        var result = JsonSerializer.Deserialize<ResultMessage>(json);
                        if (result != null)
                        {
                            await _resultProcessor.ProcessJobResultAsync(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing result message.");
                    }

                    // Acknowledge the message
                    await db.StreamAcknowledgeAsync(stream, group, entry.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Redis Stream read loop.");
                await Task.Delay(5000, stoppingToken); // Backoff on error
            }
        }
    }
}
