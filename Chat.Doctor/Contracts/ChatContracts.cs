using Clinic.Infrastructure.Entities.Enums;

namespace Chat.Doctor.Contracts;

public record ConversationResponse(
    int Id,
    string PatientId,
    string PatientName,
    DateTime? LastMessageAt,
    int UnreadCount
);

public record MessageResponse(
    int Id,
    string SenderId,
    MessageSenderType SenderType,
    string Content,
    MessageStatus Status,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public record SendMessageRequest(string Content);
