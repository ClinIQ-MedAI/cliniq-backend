using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Helpers;
using Clinic.Infrastructure.Hubs;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Services;

public class NotificationService(
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    IHubContext<NotificationHub> hubContext,
    IEmailBodyBuilder emailBodyBuilder) : INotificationService
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly IEmailBodyBuilder _emailBodyBuilder = emailBodyBuilder;

    public async Task SendNewPollsNotification(int? pollId = null)
    {
        var users = await _userManager.Users
            .Where(u => u.EmailConfirmed)
            .ToListAsync();

        foreach (var user in users)
        {
            var placeHolders = new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName }
            };

            var body = _emailBodyBuilder.GenerateEmailBody("PollNotification", placeHolders);

            await _emailSender.SendEmailAsync(user.Email!, "Clinic API: Notification", body);
        }
    }

    public async Task NotifyAdminsAsync(string title, string body, NotificationType type, string? referenceId = null)
    {
        var adminRoleIds = await _context.Roles
            .Where(r => r.Name == "Admin" || r.Name == "SuperAdmin")
            .Select(r => r.Id)
            .ToListAsync();

        var adminUserIds = await _context.UserRoles
            .Where(ur => adminRoleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .ToListAsync();

        await CreateNotificationAsync(title, body, type, adminUserIds, referenceId);
    }

    public async Task CreateNotificationAsync(string title, string body, NotificationType type, IList<string> userIds, string? referenceId = null)
    {
        var notification = new Notification
        {
            Title = title,
            Body = body,
            Type = type,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow,
            Recipients = userIds.Select(userId => new NotificationRecipient
            {
                UserId = userId,
                IsRead = false
            }).ToList()
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        foreach (var userId in userIds)
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new NotificationPayload(
                    notification.Id,
                    notification.Title,
                    notification.Body,
                    notification.Type.ToString(),
                    notification.ReferenceId,
                    false,
                    notification.CreatedAt
                ));
        }
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        await _context.NotificationRecipients
            .Where(r => r.NotificationId == notificationId && r.UserId == userId)
            .ExecuteUpdateAsync(r => r
                .SetProperty(p => p.IsRead, true)
                .SetProperty(p => p.ReadAt, DateTime.UtcNow));
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _context.NotificationRecipients
            .Where(r => r.UserId == userId && !r.IsRead)
            .ExecuteUpdateAsync(r => r
                .SetProperty(p => p.IsRead, true)
                .SetProperty(p => p.ReadAt, DateTime.UtcNow));
    }
}
