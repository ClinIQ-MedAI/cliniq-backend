using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinic.Infrastructure.Hubs;

[Authorize]
public class ChatbotHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"patient_{userId}");
        }
        await base.OnConnectedAsync();
    }
}
