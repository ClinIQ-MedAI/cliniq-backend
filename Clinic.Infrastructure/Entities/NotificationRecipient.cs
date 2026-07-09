namespace Clinic.Infrastructure.Entities;

public class NotificationRecipient
{
    public int NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
