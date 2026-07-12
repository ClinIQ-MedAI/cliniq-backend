using System.Text.Json;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Clinic.Infrastructure.Services.Queue;

public class QueueResultProcessor : IQueueResultProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<AIJobHub> _jobHubContext;
    private readonly IHubContext<ChatbotHub> _chatbotHubContext;
    private readonly ILogger<QueueResultProcessor> _logger;

    public QueueResultProcessor(
        IServiceScopeFactory scopeFactory,
        IHubContext<AIJobHub> jobHubContext,
        IHubContext<ChatbotHub> chatbotHubContext,
        ILogger<QueueResultProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _jobHubContext = jobHubContext;
        _chatbotHubContext = chatbotHubContext;
        _logger = logger;
    }

    public async Task ProcessJobResultAsync(ResultMessage result)
    {
        using var scope = _scopeFactory.CreateScope();
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
        await _jobHubContext.Clients.Group($"patient_{result.PatientId}").SendAsync("ReceiveJobResult", result);
    }

    public async Task ProcessChatReplyAsync(ChatReplyMessage reply)
    {
        using var scope = _scopeFactory.CreateScope();
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
            dbMessage.DurationMs = reply.DurationMs ?? 0;

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
            await _chatbotHubContext.Clients.Group($"patient_{reply.PatientId}").SendAsync("ReceiveChatbotReply", reply);
        }
        else
        {
            _logger.LogWarning("Received reply for unknown Chat ID: {ChatId}", reply.ChatId);
        }
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
