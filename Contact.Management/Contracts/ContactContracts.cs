namespace Contact.Management.Contracts;

public record ContactMessageResponse(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string Subject,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);

public record ContactMessageListResponse(
    List<ContactMessageResponse> Items,
    int TotalCount
);

public record AdminContactReplyRequest(
    int ContactId,
    string Reply
);
