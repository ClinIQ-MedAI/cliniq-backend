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

public class PrescriptionService : IPrescriptionService
{
    private readonly AppDbContext _context;
    private readonly IQueueService _queueService;
    private readonly IAIServiceClient _aiServiceClient;
    private readonly QueueSettings _queueSettings;
    private readonly ILogger<PrescriptionService> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public PrescriptionService(
        AppDbContext context,
        IQueueService queueService,
        IAIServiceClient aiServiceClient,
        IOptions<QueueSettings> queueSettings,
        ILogger<PrescriptionService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _queueService = queueService;
        _aiServiceClient = aiServiceClient;
        _queueSettings = queueSettings.Value;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<ParsedPrescriptionResponse>> UploadPrescriptionAsync(UploadPrescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);
        if (patient == null)
        {
            return Result.Failure<ParsedPrescriptionResponse>(Error.NotFound("Patient.NotFound", _localizer["Patient.NotFound"]));
        }

        var modality = AIModality.PRESCRIPTION;
        var jobId = Guid.NewGuid().ToString("N");
        
        var job = new AIJob
        {
            Id = jobId,
            Modality = modality,
            PatientId = request.PatientId,
            Status = "Pending",
            ImageBase64 = request.ImageBase64,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIJobs.Add(job);

        var prescription = new ParsedPrescription
        {
            PatientId = request.PatientId,
            PrescriptionImageBase64 = request.ImageBase64,
            PrescriptionImageUrl = request.ImageUrl,
            AIJobId = jobId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ParsedPrescriptions.Add(prescription);
        await _context.SaveChangesAsync(cancellationToken);

        var queueBackend = Environment.GetEnvironmentVariable("QUEUE_BACKEND") ?? _queueSettings.QueueBackend;
        bool useQueue = string.Equals(queueBackend, "redis", StringComparison.OrdinalIgnoreCase);

        if (useQueue)
        {
            try
            {
                await _queueService.PublishJobAsync(job);
                return Result.Succeed(MapToResponse(prescription, patient.User, job));
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.ErrorMessage = $"Failed to publish queue message: {ex.Message}";
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure<ParsedPrescriptionResponse>(Error.BadRequest("Queue.PublishError", string.Format(_localizer["Queue.PublishError"], ex.Message)));
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

            prescription.RawParsedText = job.ResultJson;
            if (result.Result != null)
            {
                prescription.MedicationsJson = ExtractMedicationsJson(result.Result);
            }
            
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Succeed(MapToResponse(prescription, patient.User, job));
        }
    }

    public async Task<Result<ParsedPrescriptionResponse>> GetPrescriptionAsync(int prescriptionId)
    {
        var prescription = await _context.ParsedPrescriptions
            .Include(p => p.Patient).ThenInclude(p => p.User)
            .Include(p => p.AIJob)
            .Include(p => p.Doctor).ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId);

        if (prescription == null)
        {
            return Result.Failure<ParsedPrescriptionResponse>(Error.NotFound("Prescription.NotFound", string.Format(_localizer["Prescription.NotFound"], prescriptionId)));
        }

        return Result.Succeed(MapToResponse(prescription, prescription.Patient.User, prescription.AIJob));
    }

    public async Task<IEnumerable<ParsedPrescriptionResponse>> GetPatientPrescriptionsAsync(string patientId)
    {
        var prescriptions = await _context.ParsedPrescriptions
            .Include(p => p.Patient).ThenInclude(p => p.User)
            .Include(p => p.AIJob)
            .Include(p => p.Doctor).ThenInclude(d => d!.User)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return prescriptions.Select(p => MapToResponse(p, p.Patient.User, p.AIJob));
    }

    public async Task<Result> ConfirmPrescriptionAsync(int prescriptionId, ConfirmPrescriptionRequest request)
    {
        var prescription = await _context.ParsedPrescriptions.FindAsync(prescriptionId);
        if (prescription == null)
        {
            return Result.Failure(Error.NotFound("Prescription.NotFound", string.Format(_localizer["Prescription.NotFound"], prescriptionId)));
        }

        var doctorExists = await _context.DoctorProfiles.AnyAsync(d => d.Id == request.DoctorId);
        if (!doctorExists)
        {
            return Result.Failure(Error.NotFound("Doctor.NotFound", _localizer["Doctor.NotFound"]));
        }

        prescription.DoctorId = request.DoctorId;
        prescription.DoctorNotes = request.DoctorNotes;
        prescription.MedicationsJson = request.Medications != null ? JsonSerializer.Serialize(request.Medications) : null;

        await _context.SaveChangesAsync();
        return Result.Succeed();
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

    private static ParsedPrescriptionResponse MapToResponse(ParsedPrescription prescription, ApplicationUser patientUser, AIJob? job)
    {
        object? rawText = null;
        if (!string.IsNullOrEmpty(prescription.RawParsedText))
        {
            try
            {
                rawText = JsonSerializer.Deserialize<object>(prescription.RawParsedText);
            }
            catch
            {
                rawText = prescription.RawParsedText;
            }
        }

        object? medications = null;
        if (!string.IsNullOrEmpty(prescription.MedicationsJson))
        {
            try
            {
                medications = JsonSerializer.Deserialize<object>(prescription.MedicationsJson);
            }
            catch
            {
                medications = prescription.MedicationsJson;
            }
        }

        var patientName = $"{patientUser.FirstName} {patientUser.LastName}".Trim();
        var doctorName = prescription.Doctor != null && prescription.Doctor.User != null
            ? $"{prescription.Doctor.User.FirstName} {prescription.Doctor.User.LastName}".Trim()
            : null;

        return new ParsedPrescriptionResponse(
            prescription.Id,
            prescription.PatientId,
            patientName,
            prescription.AIJobId,
            job?.Status ?? "Unknown",
            prescription.PrescriptionImageUrl,
            prescription.PrescriptionImageBase64,
            rawText,
            medications,
            prescription.DoctorId,
            doctorName,
            prescription.DoctorNotes,
            prescription.CreatedAt
        );
    }
}
