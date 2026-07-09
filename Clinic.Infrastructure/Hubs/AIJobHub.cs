using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinic.Infrastructure.Hubs;

[Authorize]
public class AIJobHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            // Automatically join group for the user's patient ID or user ID
            await Groups.AddToGroupAsync(Context.ConnectionId, $"patient_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinPatientGroup(string patientId)
    {
        // Explicit subscription to a patient group (e.g., if a doctor wants to watch a specific patient's job)
        await Groups.AddToGroupAsync(Context.ConnectionId, $"patient_{patientId}");
    }
}
