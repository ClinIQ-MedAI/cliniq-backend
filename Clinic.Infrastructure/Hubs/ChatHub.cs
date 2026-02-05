using Clinic.Infrastructure.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinic.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time chat communication between doctors and patients.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    /// <summary>
    /// Join a conversation group to receive real-time messages.
    /// </summary>
    public async Task JoinConversation(int conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(conversationId));
    }

    /// <summary>
    /// Leave a conversation group.
    /// </summary>
    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(conversationId));
    }

    private static string GetGroupName(int conversationId) => $"conversation_{conversationId}";
}

/// <summary>
/// DTO for SignalR message notifications.
/// </summary>
public record ChatMessageNotification(
    int MessageId,
    int ConversationId,
    string SenderId,
    MessageSenderType SenderType,
    string Content,
    DateTime CreatedAt
);
