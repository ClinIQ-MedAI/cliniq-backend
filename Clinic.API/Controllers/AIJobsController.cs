using System.Text.Json;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services.Queue;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Clinic.Infrastructure.Localization;

namespace Clinic.API.Controllers;

[Authorize]
[ApiController]
[Route("ai/jobs")]
public class AIJobsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IQueueService _queueService;
    private readonly IAIServiceClient _aiServiceClient;
    private readonly QueueSettings _queueSettings;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IQStashSignatureVerifier _signatureVerifier;
    private readonly IQueueResultProcessor _resultProcessor;

    public AIJobsController(
        AppDbContext context,
        IQueueService queueService,
        IAIServiceClient aiServiceClient,
        IOptions<QueueSettings> queueSettings,
        IStringLocalizer<SharedResource> localizer,
        IQStashSignatureVerifier signatureVerifier,
        IQueueResultProcessor resultProcessor)
    {
        _context = context;
        _queueService = queueService;
        _aiServiceClient = aiServiceClient;
        _queueSettings = queueSettings.Value;
        _localizer = localizer;
        _signatureVerifier = signatureVerifier;
        _resultProcessor = resultProcessor;
    }

    public record SubmitJobRequest(
        AIModality Modality,
        string? ImageBase64,
        string? ImageUrl,
        string PatientId,
        object? Options
    );

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitJob([FromBody] SubmitJobRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64) && string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            return BadRequest(new { Message = _localizer["Validation.ImageRequired"].Value });
        }

        if (string.IsNullOrWhiteSpace(request.PatientId))
        {
            return BadRequest(new { Message = _localizer["Validation.PatientIdRequired"].Value });
        }

        if (request.Options != null && request.Modality != AIModality.CHEST && request.Modality != AIModality.DENTAL_PHOTO)
        {
            return BadRequest(new { Message = _localizer["Validation.OptionsNotAllowed"].Value });
        }

        var jobId = Guid.NewGuid().ToString("N");
        var job = new AIJob
        {
            Id = jobId,
            Modality = request.Modality,
            PatientId = request.PatientId,
            Status = "Pending",
            ImageBase64 = request.ImageBase64,
            ImageUrl = request.ImageUrl,
            OptionsJson = request.Options != null ? JsonSerializer.Serialize(request.Options) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        bool useQueue = _queueService.IsEnabled;

        if (useQueue)
        {
            try
            {
                await _queueService.PublishJobAsync(job);
                return Accepted(new { JobId = jobId, Status = "Pending", Mode = "queue" });
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.ErrorMessage = $"Failed to enqueue job: {ex.Message}";
                await _context.SaveChangesAsync(cancellationToken);
                return StatusCode(500, new { Message = "Failed to queue the AI job.", Error = ex.Message });
            }
        }
        else
        {
            var jobMessage = new JobMessage
            {
                JobId = jobId,
                Modality = request.Modality.ToString().ToLowerInvariant(),
                ImageBase64 = request.ImageBase64,
                ImageUrl = request.ImageUrl,
                PatientId = request.PatientId,
                Options = request.Options,
                EnqueuedAt = job.CreatedAt.ToString("o")
            };

            var result = await _aiServiceClient.SendPredictRequestAsync(request.Modality.ToString().ToLowerInvariant(), jobMessage, cancellationToken);
            
            job.Status = result.Status;
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

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                JobId = jobId,
                Status = job.Status,
                Mode = "http",
                Result = result.Result,
                Error = result.Error
            });
        }
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetJobStatus([FromRoute] string jobId)
    {
        var job = await _context.AIJobs.FindAsync(jobId);
        if (job == null)
        {
            return NotFound(new { Message = $"Job with ID '{jobId}' not found." });
        }

        object? resultPayload = null;
        if (!string.IsNullOrEmpty(job.ResultJson))
        {
            try
            {
                resultPayload = JsonSerializer.Deserialize<object>(job.ResultJson);
            }
            catch
            {
                resultPayload = job.ResultJson;
            }
        }

        return Ok(new
        {
            JobId = job.Id,
            Modality = job.Modality,
            PatientId = job.PatientId,
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            FinishedAt = job.FinishedAt,
            DurationMs = job.DurationMs,
            Worker = job.Worker,
            Result = resultPayload,
            Error = job.ErrorMessage
        });
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetPatientJobs([FromRoute] string patientId)
    {
        var jobs = await _context.AIJobs
            .Where(j => j.PatientId == patientId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return Ok(jobs.Select(job =>
        {
            object? resultPayload = null;
            if (!string.IsNullOrEmpty(job.ResultJson))
            {
                try
                {
                    resultPayload = JsonSerializer.Deserialize<object>(job.ResultJson);
                }
                catch
                {
                    resultPayload = job.ResultJson;
                }
            }

            return new
            {
                JobId = job.Id,
                Modality = job.Modality,
                PatientId = job.PatientId,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                FinishedAt = job.FinishedAt,
                DurationMs = job.DurationMs,
                Worker = job.Worker,
                Result = resultPayload,
                Error = job.ErrorMessage
            };
        }));
    }

    public record QStashCallbackPayload(
        int Status,
        string? Body,
        int Retried,
        int MaxRetries,
        string? SourceMessageId
    );

    [HttpPost("callback/job")]
    [AllowAnonymous]
    public async Task<IActionResult> JobCallback()
    {
        // 1. Read raw body for signature verification
        using var reader = new System.IO.StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        // 2. Retrieve signature header
        var signature = Request.Headers["Upstash-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            return Unauthorized("Missing Upstash-Signature header.");
        }

        // Get current URL
        var currentUrl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(Request);

        // 3. Verify signature
        if (!_signatureVerifier.Verify(signature, rawBody, currentUrl))
        {
            return Unauthorized("Invalid Upstash-Signature.");
        }

        // 4. Deserialize callback payload
        QStashCallbackPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<QStashCallbackPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid payload format: {ex.Message}");
        }

        if (payload == null)
        {
            return BadRequest("Payload is empty.");
        }

        if (string.IsNullOrEmpty(payload.Body))
        {
            return BadRequest("Response body is empty.");
        }

        // 5. Decode base64 body from destination
        string decodedJson;
        try
        {
            var bytes = Convert.FromBase64String(payload.Body);
            decodedJson = System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to decode base64 body: {ex.Message}");
        }

        // 6. Deserialize original response to ResultMessage
        ResultMessage? result;
        try
        {
            result = JsonSerializer.Deserialize<ResultMessage>(decodedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to deserialize result message: {ex.Message}");
        }

        if (result == null)
        {
            return BadRequest("Result message is empty.");
        }

        // 7. Process result
        await _resultProcessor.ProcessJobResultAsync(result);

        return Ok();
    }

    [HttpPost("callback/chat")]
    [AllowAnonymous]
    public async Task<IActionResult> ChatCallback()
    {
        // 1. Read raw body for signature verification
        using var reader = new System.IO.StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        // 2. Retrieve signature header
        var signature = Request.Headers["Upstash-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            return Unauthorized("Missing Upstash-Signature header.");
        }

        // Get current URL
        var currentUrl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(Request);

        // 3. Verify signature
        if (!_signatureVerifier.Verify(signature, rawBody, currentUrl))
        {
            return Unauthorized("Invalid Upstash-Signature.");
        }

        // 4. Deserialize callback payload
        QStashCallbackPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<QStashCallbackPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid payload format: {ex.Message}");
        }

        if (payload == null)
        {
            return BadRequest("Payload is empty.");
        }

        if (string.IsNullOrEmpty(payload.Body))
        {
            return BadRequest("Response body is empty.");
        }

        // 5. Decode base64 body from destination
        string decodedJson;
        try
        {
            var bytes = Convert.FromBase64String(payload.Body);
            decodedJson = System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to decode base64 body: {ex.Message}");
        }

        // 6. Deserialize original response to ChatReplyMessage
        ChatReplyMessage? reply;
        try
        {
            reply = JsonSerializer.Deserialize<ChatReplyMessage>(decodedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to deserialize chatbot reply: {ex.Message}");
        }

        if (reply == null)
        {
            return BadRequest("Chatbot reply is empty.");
        }

        // 7. Process chat reply
        await _resultProcessor.ProcessChatReplyAsync(reply);

        return Ok();
    }
}
