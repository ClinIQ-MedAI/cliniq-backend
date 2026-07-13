using System.Text.Json;
using Clinic.AIFeatures.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services.Queue;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Clinic.Infrastructure.Localization;

namespace Clinic.AIFeatures.Services;

public class ScanService : IScanService
{
    private readonly AppDbContext _context;
    private readonly IQueueService _queueService;
    private readonly IAIServiceClient _aiServiceClient;
    private readonly QueueSettings _queueSettings;
    private readonly ILogger<ScanService> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ScanService(
        AppDbContext context,
        IQueueService queueService,
        IAIServiceClient aiServiceClient,
        IOptions<QueueSettings> queueSettings,
        ILogger<ScanService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _queueService = queueService;
        _aiServiceClient = aiServiceClient;
        _queueSettings = queueSettings.Value;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<ScanResponse>> UploadScanAsync(UploadScanRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);
        if (patient == null)
        {
            return Result.Failure<ScanResponse>(Error.NotFound("Patient.NotFound", _localizer["Patient.NotFound"]));
        }

        var modality = request.Modality;

        if (request.Options != null && modality != AIModality.CHEST && modality != AIModality.DENTAL_PHOTO)
        {
            return Result.Failure<ScanResponse>(Error.BadRequest("Scan.OptionsNotAllowed", _localizer["Validation.OptionsNotAllowed"]));
        }

        var jobId = Guid.NewGuid().ToString("N");
        
        var job = new AIJob
        {
            Id = jobId,
            Modality = modality,
            PatientId = request.PatientId,
            Status = "Pending",
            ImageBase64 = request.ImageBase64,
            ImageUrl = request.ImageUrl,
            OptionsJson = request.Options != null ? JsonSerializer.Serialize(request.Options) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIJobs.Add(job);

        var scan = new PatientScan
        {
            PatientId = request.PatientId,
            Modality = modality,
            ScanUrl = request.ImageUrl,
            ScanBase64 = request.ImageBase64,
            AIJobId = jobId,
            IsReviewed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PatientScans.Add(scan);
        await _context.SaveChangesAsync(cancellationToken);

        bool useQueue = _queueService.IsEnabled;

        if (useQueue)
        {
            try
            {
                await _queueService.PublishJobAsync(job);
                return Result.Succeed(MapToResponse(scan, patient.User, job));
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.ErrorMessage = $"Failed to publish queue message: {ex.Message}";
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure<ScanResponse>(Error.BadRequest("Queue.PublishError", string.Format(_localizer["Queue.PublishError"], ex.Message)));
            }
        }
        else
        {
            var jobMessage = new JobMessage
            {
                JobId = jobId,
                Modality = modality.ToString().ToLowerInvariant(),
                ImageBase64 = request.ImageBase64,
                ImageUrl = request.ImageUrl,
                PatientId = request.PatientId,
                Options = request.Options,
                EnqueuedAt = job.CreatedAt.ToString("o")
            };

            var result = await _aiServiceClient.SendPredictRequestAsync(modality.ToString().ToLowerInvariant(), jobMessage, cancellationToken);
            
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

            scan.AIAnalysisResult = job.ResultJson;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Succeed(MapToResponse(scan, patient.User, job));
        }
    }

    public async Task<Result<ScanResponse>> GetScanAsync(int scanId)
    {
        var scan = await _context.PatientScans
            .Include(s => s.Patient).ThenInclude(p => p.User)
            .Include(s => s.AIJob)
            .Include(s => s.Doctor).ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(s => s.Id == scanId);

        if (scan == null)
        {
            return Result.Failure<ScanResponse>(Error.NotFound("Scan.NotFound", string.Format(_localizer["Scan.NotFound"], scanId)));
        }

        return Result.Succeed(MapToResponse(scan, scan.Patient.User, scan.AIJob));
    }

    public async Task<IEnumerable<ScanResponse>> GetPatientScansAsync(string patientId)
    {
        var scans = await _context.PatientScans
            .Include(s => s.Patient).ThenInclude(p => p.User)
            .Include(s => s.AIJob)
            .Include(s => s.Doctor).ThenInclude(d => d!.User)
            .Where(s => s.PatientId == patientId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return scans.Select(s => MapToResponse(s, s.Patient.User, s.AIJob));
    }

    public async Task<Result> ReviewScanAsync(int scanId, ReviewScanRequest request)
    {
        var scan = await _context.PatientScans.FindAsync(scanId);
        if (scan == null)
        {
            return Result.Failure(Error.NotFound("Scan.NotFound", string.Format(_localizer["Scan.NotFound"], scanId)));
        }

        var doctorExists = await _context.DoctorProfiles.AnyAsync(d => d.Id == request.DoctorId);
        if (!doctorExists)
        {
            return Result.Failure(Error.NotFound("Doctor.NotFound", _localizer["Doctor.NotFound"]));
        }

        scan.DoctorId = request.DoctorId;
        scan.DoctorNotes = request.DoctorNotes;
        scan.DoctorReviewDate = DateTime.UtcNow;
        scan.IsReviewed = true;

        await _context.SaveChangesAsync();
        return Result.Succeed();
    }

    public async Task<Result> ConfirmScanAsync(int scanId, ConfirmScanRequest request)
    {
        var scan = await _context.PatientScans.FindAsync(scanId);
        if (scan == null)
        {
            return Result.Failure(Error.NotFound("Scan.NotFound", string.Format(_localizer["Scan.NotFound"], scanId)));
        }

        var doctorExists = await _context.DoctorProfiles.AnyAsync(d => d.Id == request.DoctorId);
        if (!doctorExists)
        {
            return Result.Failure(Error.NotFound("Doctor.NotFound", _localizer["Doctor.NotFound"]));
        }

        scan.DoctorId = request.DoctorId;
        scan.DoctorNotes = request.DoctorNotes;
        scan.DoctorReviewDate = DateTime.UtcNow;
        scan.IsReviewed = true;

        if (request.Medications != null)
        {
            scan.AIAnalysisResult = JsonSerializer.Serialize(request.Medications);
        }

        await _context.SaveChangesAsync();
        return Result.Succeed();
    }

    private static ScanResponse MapToResponse(PatientScan scan, ApplicationUser patientUser, AIJob? job)
    {
        object? aiResult = null;
        if (!string.IsNullOrEmpty(scan.AIAnalysisResult))
        {
            try
            {
                aiResult = JsonSerializer.Deserialize<object>(scan.AIAnalysisResult);
            }
            catch
            {
                aiResult = scan.AIAnalysisResult;
            }
        }

        var patientName = $"{patientUser.FirstName} {patientUser.LastName}".Trim();
        var doctorName = scan.Doctor != null && scan.Doctor.User != null
            ? $"{scan.Doctor.User.FirstName} {scan.Doctor.User.LastName}".Trim()
            : null;

        return new ScanResponse(
            scan.Id,
            scan.PatientId,
            patientName,
            scan.Modality,
            scan.ScanUrl,
            scan.ScanBase64,
            scan.AIJobId,
            job?.Status ?? "Unknown",
            aiResult,
            scan.DoctorId,
            doctorName,
            scan.DoctorNotes,
            scan.DoctorReviewDate,
            scan.IsReviewed,
            scan.CreatedAt
        );
    }
}
