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

    public AIJobsController(
        AppDbContext context,
        IQueueService queueService,
        IAIServiceClient aiServiceClient,
        IOptions<QueueSettings> queueSettings,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _queueService = queueService;
        _aiServiceClient = aiServiceClient;
        _queueSettings = queueSettings.Value;
        _localizer = localizer;
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

        var queueBackend = Environment.GetEnvironmentVariable("QUEUE_BACKEND") ?? _queueSettings.QueueBackend;
        bool useQueue = string.Equals(queueBackend, "redis", StringComparison.OrdinalIgnoreCase);

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
}
