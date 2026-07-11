using Clinic.Authentication.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Management.Contracts;

namespace Notification.Management.Controllers;

[ApiController]
[Route("admin/notifications")]
public class NotificationManagementController(
    Services.INotificationManagementService notificationService) : ControllerBase
{
    private readonly Services.INotificationManagementService _notificationService = notificationService;

    [HttpPost("send")]
    [HasPermission(Permissions.SendNotifications)]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
    {
        var result = await _notificationService.SendToUsersAsync(request);
        return result.IsSucceed ? Ok() : result.ToProblem();
    }
}
