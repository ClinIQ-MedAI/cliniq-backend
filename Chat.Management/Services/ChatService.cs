using Chat.Management.Contracts;
using Chat.Management.Localization;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Chat.Management.Services;

public class ChatService(
    AppDbContext context,
    IStringLocalizer<Messages> localizer) : IChatService
{
    private readonly AppDbContext _context = context;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    public async Task<Result<IEnumerable<ConversationResponse>>> GetAllConversationsAsync(CancellationToken ct = default)
    {
        var conversations = await _context.Conversations
            .Select(c => new ConversationResponse(
                c.Id,
                c.DoctorId,
                c.Doctor.User.FirstName + " " + c.Doctor.User.LastName,
                c.PatientId,
                c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                c.LastMessageAt,
                c.Messages.Count
            ))
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(ct);

        return Result.Succeed<IEnumerable<ConversationResponse>>(conversations);
    }

    public async Task<Result<IEnumerable<MessageResponse>>> GetMessagesAsync(int conversationId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conversation == null)
            return Result.Failure<IEnumerable<MessageResponse>>(Error.NotFound("Conversation.NotFound", _localizer["ConversationNotFound"]));

        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse(
                m.Id,
                m.SenderId,
                m.Sender.FirstName + " " + m.Sender.LastName,
                m.SenderType,
                m.Content,
                m.Status,
                m.CreatedAt,
                m.ReadAt
            ))
            .ToListAsync(ct);

        return Result.Succeed<IEnumerable<MessageResponse>>(messages);
    }
}
