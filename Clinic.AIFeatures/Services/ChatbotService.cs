using System.Text.Json;
using Clinic.AIFeatures.Contracts;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services.Queue;
using Clinic.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clinic.AIFeatures.Services;

public class ChatbotService : IChatbotService
{
    private readonly AppDbContext _context;
    private readonly IQueueService _queueService;
    private readonly QueueSettings _queueSettings;

    public ChatbotService(
        AppDbContext context,
        IQueueService queueService,
        IOptions<QueueSettings> queueSettings)
    {
        _context = context;
        _queueService = queueService;
        _queueSettings = queueSettings.Value;
    }

    public async Task<Result<ChatbotResponse>> SendMessageAsync(
        string patientId,
        ChatbotRequest request,
        CancellationToken ct)
    {
        // 1. Verify patient exists
        var patientExists = await _context.PatientProfiles.AnyAsync(p => p.Id == patientId, ct);
        if (!patientExists)
        {
            return Result.Failure<ChatbotResponse>(Error.NotFound("Patient.NotFound", "Patient profile not found."));
        }

        // 2. Create message database record
        var chatId = Guid.NewGuid().ToString("N");
        var chatMessage = new AIChatMessage
        {
            ChatId = chatId,
            PatientId = patientId,
            Message = request.Message,
            LanguagePreference = request.LanguagePreference,
            Status = "Pending"
        };

        _context.AIChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync(ct);

        // 3. Publish to queue if enabled
        if (_queueService.IsEnabled)
        {
            await _queueService.PublishChatAsync(chatId, patientId, request.Message, request.LanguagePreference);
        }

        return Result.Succeed(MapToResponse(chatMessage));
    }

    public async Task<Result<IEnumerable<ChatbotResponse>>> GetChatHistoryAsync(
        string patientId,
        CancellationToken ct)
    {
        var history = await _context.AIChatMessages
            .Where(m => m.PatientId == patientId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToResponse(m))
            .ToListAsync(ct);

        return Result.Succeed<IEnumerable<ChatbotResponse>>(history);
    }

    private static ChatbotResponse MapToResponse(AIChatMessage msg) => new()
    {
        Id = msg.Id,
        ChatId = msg.ChatId,
        PatientId = msg.PatientId,
        Message = msg.Message,
        LanguagePreference = msg.LanguagePreference,
        Status = msg.Status,
        Reply = msg.Reply,
        QueryType = msg.QueryType,
        ShowUpload = msg.ShowUpload,
        Error = msg.Error,
        CreatedAt = msg.CreatedAt,
        FinishedAt = msg.FinishedAt
    };
}
