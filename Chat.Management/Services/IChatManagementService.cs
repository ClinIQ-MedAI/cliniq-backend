using Chat.Management.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Chat.Management.Services;

public interface IChatManagementService
{
    Task<Result<IEnumerable<ConversationResponse>>> GetAllConversationsAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<MessageResponse>>> GetMessagesAsync(int conversationId, CancellationToken ct = default);
}
