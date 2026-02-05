using Clinic.Infrastructure.Entities.Enums;

namespace Chat.Patient.Contracts;

public record ConversationResponse(
    int Id,
    string DoctorId,
    string DoctorName,
    string? DoctorSpecialization,
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

public record StartConversationRequest(string DoctorId, string InitialMessage);
