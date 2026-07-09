using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinic.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        await base.OnConnectedAsync();
    }
}

public record NotificationPayload(
    int Id,
    string Title,
    string Body,
    string Type,
    string? ReferenceId,
    bool IsRead,
    DateTime CreatedAt
);
