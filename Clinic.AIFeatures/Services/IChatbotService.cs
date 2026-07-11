using Clinic.AIFeatures.Contracts;
using Clinic.Infrastructure.Abstractions;

namespace Clinic.AIFeatures.Services;

public interface IChatbotService
{
    Task<Result<ChatbotResponse>> SendMessageAsync(string patientId, ChatbotRequest request, CancellationToken ct);
    Task<Result<IEnumerable<ChatbotResponse>>> GetChatHistoryAsync(string patientId, CancellationToken ct);
}
