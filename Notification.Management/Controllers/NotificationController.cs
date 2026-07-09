using Clinic.Authentication.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Management.Contracts;

namespace Notification.Management.Controllers;

[ApiController]
[Route("admin/notifications")]
[Authorize(Policy = PolicyNames.Admin)]
public class NotificationController(
    Services.INotificationService notificationService) : ControllerBase
{
    private readonly Services.INotificationService _notificationService = notificationService;

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
    {
        var result = await _notificationService.SendToUsersAsync(request);
        return result.IsSucceed ? Ok() : result.ToProblem();
    }
}
