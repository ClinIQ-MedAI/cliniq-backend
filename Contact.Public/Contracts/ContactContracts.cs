namespace Contact.Public.Contracts;

public record ContactUsRequest(
    string Name,
    string Email,
    string? Phone,
    string Subject,
    string Message
);
