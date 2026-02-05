using Clinic.Infrastructure.Entities.Enums;

namespace Chat.Management.Contracts;

public record ConversationResponse(
    int Id,
    string DoctorId,
    string DoctorName,
    string PatientId,
    string PatientName,
    DateTime? LastMessageAt,
    int MessageCount
);

public record MessageResponse(
    int Id,
    string SenderId,
    string SenderName,
    MessageSenderType SenderType,
    string Content,
    MessageStatus Status,
    DateTime CreatedAt,
    DateTime? ReadAt
);
