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
    private readonly IHubContext<AIJobHub> _hubContext;

    public QueueConsumerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<QueueConsumerBackgroundService> logger,
        IOptions<QueueSettings> settings,
        IHubContext<AIJobHub> hubContext)
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
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

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
                            await ProcessResultMessageAsync(result);
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

    private async Task ProcessResultMessageAsync(ResultMessage result)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var job = await context.AIJobs.FindAsync(result.JobId);
        if (job != null)
        {
            var status = result.Status;
            if (!string.IsNullOrEmpty(status))
            {
                status = char.ToUpper(status[0]) + status.Substring(1).ToLowerInvariant();
            }

            job.Status = status;
            job.ResultJson = result.Result != null ? JsonSerializer.Serialize(result.Result) : null;
            job.ErrorMessage = result.Error;
            job.Worker = result.Worker;
            job.DurationMs = result.DurationMs;
            
            if (DateTime.TryParse(result.FinishedAt, out var finishedAt))
            {
                job.FinishedAt = finishedAt;
            }
            else
            {
                job.FinishedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("AI Job {JobId} updated successfully to status {Status}", job.Id, job.Status);

            // Propagate updates to PatientScan if one matches the JobId
            var scan = await context.PatientScans.FirstOrDefaultAsync(s => s.AIJobId == result.JobId);
            if (scan != null)
            {
                scan.AIAnalysisResult = job.ResultJson;
                scan.IsReviewed = false;
                await context.SaveChangesAsync();
                _logger.LogInformation("PatientScan linked to job {JobId} updated with AI analysis.", result.JobId);
            }

            // Propagate updates to ParsedPrescription if one matches the JobId
            var prescription = await context.ParsedPrescriptions.FirstOrDefaultAsync(p => p.AIJobId == result.JobId);
            if (prescription != null)
            {
                prescription.RawParsedText = job.ResultJson;
                if (result.Result != null)
                {
                    prescription.MedicationsJson = ExtractMedicationsJson(result.Result);
                }
                await context.SaveChangesAsync();
                _logger.LogInformation("ParsedPrescription linked to job {JobId} updated with parsed text.", result.JobId);
            }
        }
        else
        {
            _logger.LogWarning("Received result for unknown Job ID: {JobId}", result.JobId);
        }

        // Push to clients via SignalR patient group
        await _hubContext.Clients.Group($"patient_{result.PatientId}").SendAsync("ReceiveJobResult", result);
    }

    private string? ExtractMedicationsJson(object resultObj)
    {
        if (resultObj == null) return null;

        try
        {
            var jsonString = JsonSerializer.Serialize(resultObj);
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            if (root.TryGetProperty("medications", out var medsElement) && medsElement.ValueKind == JsonValueKind.Array)
            {
                return medsElement.GetRawText();
            }
            if (root.TryGetProperty("medicines", out var medsElement2) && medsElement2.ValueKind == JsonValueKind.Array)
            {
                return medsElement2.GetRawText();
            }
            if (root.TryGetProperty("medication", out var medsElement3) && medsElement3.ValueKind == JsonValueKind.Array)
            {
                return medsElement3.GetRawText();
            }
            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.GetRawText();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract medications from AI result.");
        }

        return null;
    }
}
