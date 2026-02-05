using Chat.Doctor.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Chat.Doctor.Services;

public interface IChatService
{
    Task<Result<IEnumerable<ConversationResponse>>> GetConversationsAsync(string doctorId, CancellationToken ct = default);
    Task<Result<IEnumerable<MessageResponse>>> GetMessagesAsync(string doctorId, int conversationId, CancellationToken ct = default);
    Task<Result<MessageResponse>> SendMessageAsync(string doctorId, int conversationId, SendMessageRequest request, CancellationToken ct = default);
}
