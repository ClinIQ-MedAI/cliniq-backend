using Clinic.Infrastructure.Abstractions;
using Notification.User.Contracts;

namespace Notification.User.Services;

public interface INotificationService
{
    Task<Result<List<NotificationResponse>>> GetAllAsync(string userId);
    Task<Result<UnreadCountResponse>> GetUnreadCountAsync(string userId);
    Task<Result> MarkAsReadAsync(int notificationId, string userId);
    Task<Result> MarkAllAsReadAsync(string userId);
}
