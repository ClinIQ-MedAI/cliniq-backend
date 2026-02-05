using Chat.Patient.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Chat.Patient.Services;

public interface IChatService
{
    Task<Result<IEnumerable<ConversationResponse>>> GetConversationsAsync(string patientId, CancellationToken ct = default);
    Task<Result<IEnumerable<MessageResponse>>> GetMessagesAsync(string patientId, int conversationId, CancellationToken ct = default);
    Task<Result<ConversationResponse>> StartConversationAsync(string patientId, StartConversationRequest request, CancellationToken ct = default);
    Task<Result<MessageResponse>> SendMessageAsync(string patientId, int conversationId, SendMessageRequest request, CancellationToken ct = default);
}
