using Chat.Patient.Contracts;
using Chat.Patient.Localization;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Chat.Patient.Services;

public class ChatService(
    AppDbContext context,
    IHubContext<ChatHub> hubContext,
    IStringLocalizer<Messages> localizer) : IChatService
{
    private readonly AppDbContext _context = context;
    private readonly IHubContext<ChatHub> _hubContext = hubContext;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    public async Task<Result<IEnumerable<ConversationResponse>>> GetConversationsAsync(string patientId, CancellationToken ct = default)
    {
        var conversations = await _context.Conversations
            .Where(c => c.PatientId == patientId)
            .Select(c => new ConversationResponse(
                c.Id,
                c.DoctorId,
                c.Doctor.User.FirstName + " " + c.Doctor.User.LastName,
                c.Doctor.Specialization,
                c.LastMessageAt,
                c.Messages.Count(m => m.Status != MessageStatus.READ && m.SenderType == MessageSenderType.DOCTOR)
            ))
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(ct);

        return Result.Succeed<IEnumerable<ConversationResponse>>(conversations);
    }

    public async Task<Result<IEnumerable<MessageResponse>>> GetMessagesAsync(string patientId, int conversationId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.PatientId == patientId, ct);

        if (conversation == null)
            return Result.Failure<IEnumerable<MessageResponse>>(Error.NotFound("Conversation.NotFound", _localizer["ConversationNotFound"]));

        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse(
                m.Id,
                m.SenderId,
                m.SenderType,
                m.Content,
                m.Status,
                m.CreatedAt,
                m.ReadAt
            ))
            .ToListAsync(ct);

        // Mark doctor messages as read
        await _context.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderType == MessageSenderType.DOCTOR && m.Status != MessageStatus.READ)
            .ExecuteUpdateAsync(m => m
                .SetProperty(p => p.Status, MessageStatus.READ)
                .SetProperty(p => p.ReadAt, DateTime.UtcNow), ct);

        return Result.Succeed<IEnumerable<MessageResponse>>(messages);
    }

    public async Task<Result<ConversationResponse>> StartConversationAsync(string patientId, StartConversationRequest request, CancellationToken ct = default)
    {
        // Check if doctor exists
        var doctor = await _context.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, ct);

        if (doctor == null)
            return Result.Failure<ConversationResponse>(Error.NotFound("Doctor.NotFound", _localizer["DoctorNotFound"]));

        // Check if conversation already exists
        var existingConversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.DoctorId == request.DoctorId && c.PatientId == patientId, ct);

        if (existingConversation != null)
            return Result.Failure<ConversationResponse>(Error.Conflict("Conversation.AlreadyExists", _localizer["ConversationAlreadyExists"]));

        var conversation = new Conversation
        {
            DoctorId = request.DoctorId,
            PatientId = patientId,
            LastMessageAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(ct);

        // Add initial message
        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = patientId,
            SenderType = MessageSenderType.PATIENT,
            Content = request.InitialMessage,
            Status = MessageStatus.SENT
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(ct);

        // Send real-time notification via SignalR
        await _hubContext.Clients.Group($"conversation_{conversation.Id}")
            .SendAsync("ReceiveMessage", new ChatMessageNotification(
                message.Id,
                conversation.Id,
                message.SenderId,
                message.SenderType,
                message.Content,
                message.CreatedAt
            ), ct);

        return Result.Succeed(new ConversationResponse(
            conversation.Id,
            conversation.DoctorId,
            doctor.User.FirstName + " " + doctor.User.LastName,
            doctor.Specialization,
            conversation.LastMessageAt,
            0
        ));
    }

    public async Task<Result<MessageResponse>> SendMessageAsync(string patientId, int conversationId, SendMessageRequest request, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.PatientId == patientId, ct);

        if (conversation == null)
            return Result.Failure<MessageResponse>(Error.NotFound("Conversation.NotFound", _localizer["ConversationNotFound"]));

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = patientId,
            SenderType = MessageSenderType.PATIENT,
            Content = request.Content,
            Status = MessageStatus.SENT
        };

        _context.Messages.Add(message);
        conversation.LastMessageAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var response = new MessageResponse(
            message.Id,
            message.SenderId,
            message.SenderType,
            message.Content,
            message.Status,
            message.CreatedAt,
            message.ReadAt
        );

        // Send real-time notification via SignalR
        await _hubContext.Clients.Group($"conversation_{conversationId}")
            .SendAsync("ReceiveMessage", new ChatMessageNotification(
                message.Id,
                conversationId,
                message.SenderId,
                message.SenderType,
                message.Content,
                message.CreatedAt
            ), ct);

        return Result.Succeed(response);
    }
}
