using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Represents a single message within a conversation.
/// </summary>
public class Message : AuditableEntity
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    // Who sent this message
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public MessageStatus Status { get; set; } = MessageStatus.SENT;

    public MessageSenderType SenderType { get; set; }

    public DateTime? ReadAt { get; set; }
}
