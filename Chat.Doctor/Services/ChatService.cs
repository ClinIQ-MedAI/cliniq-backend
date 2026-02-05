using Chat.Doctor.Contracts;
using Chat.Doctor.Localization;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Chat.Doctor.Services;

public class ChatService(
    AppDbContext context,
    IHubContext<ChatHub> hubContext,
    IStringLocalizer<Messages> localizer) : IChatService
{
    private readonly AppDbContext _context = context;
    private readonly IHubContext<ChatHub> _hubContext = hubContext;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    public async Task<Result<IEnumerable<ConversationResponse>>> GetConversationsAsync(string doctorId, CancellationToken ct = default)
    {
        var conversations = await _context.Conversations
            .Where(c => c.DoctorId == doctorId)
            .Select(c => new ConversationResponse(
                c.Id,
                c.PatientId,
                c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                c.LastMessageAt,
                c.Messages.Count(m => m.Status != MessageStatus.READ && m.SenderType == MessageSenderType.PATIENT)
            ))
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(ct);

        return Result.Succeed<IEnumerable<ConversationResponse>>(conversations);
    }

    public async Task<Result<IEnumerable<MessageResponse>>> GetMessagesAsync(string doctorId, int conversationId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.DoctorId == doctorId, ct);

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

        // Mark patient messages as read
        await _context.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderType == MessageSenderType.PATIENT && m.Status != MessageStatus.READ)
            .ExecuteUpdateAsync(m => m
                .SetProperty(p => p.Status, MessageStatus.READ)
                .SetProperty(p => p.ReadAt, DateTime.UtcNow), ct);

        return Result.Succeed<IEnumerable<MessageResponse>>(messages);
    }

    public async Task<Result<MessageResponse>> SendMessageAsync(string doctorId, int conversationId, SendMessageRequest request, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.DoctorId == doctorId, ct);

        if (conversation == null)
            return Result.Failure<MessageResponse>(Error.NotFound("Conversation.NotFound", _localizer["ConversationNotFound"]));

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = doctorId,
            SenderType = MessageSenderType.DOCTOR,
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
