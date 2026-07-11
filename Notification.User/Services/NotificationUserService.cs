using Clinic.Infrastructure.Abstractions;
using Notification.User.Contracts;

namespace Notification.User.Services;

public class NotificationUserService(
    AppDbContext context) : INotificationUserService
{
    private readonly AppDbContext _context = context;

    public async Task<Result<List<NotificationResponse>>> GetAllAsync(string userId)
    {
        var notifications = await _context.NotificationRecipients
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Notification.CreatedAt)
            .Select(r => new NotificationResponse(
                r.NotificationId,
                r.Notification.Title,
                r.Notification.Body,
                r.Notification.Type.ToString(),
                r.Notification.ReferenceId,
                r.IsRead,
                r.Notification.CreatedAt
            ))
            .AsNoTracking()
            .ToListAsync();

        return Result.Succeed(notifications);
    }

    public async Task<Result<UnreadCountResponse>> GetUnreadCountAsync(string userId)
    {
        var count = await _context.NotificationRecipients
            .CountAsync(r => r.UserId == userId && !r.IsRead);

        return Result.Succeed(new UnreadCountResponse(count));
    }

    public async Task<Result> MarkAsReadAsync(int notificationId, string userId)
    {
        var recipient = await _context.NotificationRecipients
            .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId);

        if (recipient is null)
            return Result.Failure(Error.NotFound("Notification.NotFound", "Notification not found"));

        recipient.IsRead = true;
        recipient.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Succeed();
    }

    public async Task<Result> MarkAllAsReadAsync(string userId)
    {
        await _context.NotificationRecipients
            .Where(r => r.UserId == userId && !r.IsRead)
            .ExecuteUpdateAsync(r => r
                .SetProperty(p => p.IsRead, true)
                .SetProperty(p => p.ReadAt, DateTime.UtcNow));

        return Result.Succeed();
    }
}
