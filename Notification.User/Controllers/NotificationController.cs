using Clinic.Authentication.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.User.Services;

namespace Notification.User.Controllers;

[ApiController]
[Route("notifications")]
[Authorize]
public class NotificationController(
    INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId()!;
        var result = await _notificationService.GetAllAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.GetUserId()!;
        var result = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(result.Value);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.GetUserId()!;
        var result = await _notificationService.MarkAsReadAsync(id, userId);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.GetUserId()!;
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}
