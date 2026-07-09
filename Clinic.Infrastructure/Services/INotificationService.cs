using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Services;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
    Task CreateNotificationAsync(string title, string body, NotificationType type, IList<string> userIds, string? referenceId = null);
    Task NotifyAdminsAsync(string title, string body, NotificationType type, string? referenceId = null);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}
