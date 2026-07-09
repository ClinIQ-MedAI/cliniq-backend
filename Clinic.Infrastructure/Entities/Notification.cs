using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Entities;

public class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public string? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<NotificationRecipient> Recipients { get; set; } = [];
}
