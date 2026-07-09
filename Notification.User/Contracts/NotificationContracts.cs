namespace Notification.User.Contracts;

public record NotificationResponse(
    int Id,
    string Title,
    string Body,
    string Type,
    string? ReferenceId,
    bool IsRead,
    DateTime CreatedAt
);

public record UnreadCountResponse(
    int Count
);
