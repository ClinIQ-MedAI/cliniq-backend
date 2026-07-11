using Contact.Public.Contracts;

namespace Contact.Public.Services;

public class ContactPublicService(
    AppDbContext context,
    INotificationService notificationService) : IContactPublicService
{
    private readonly AppDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result> SubmitAsync(ContactUsRequest request)
    {
        var message = new ContactUsMessage
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject,
            Body = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContactUsMessages.Add(message);
        await _context.SaveChangesAsync();

        await _notificationService.NotifyAdminsAsync(
            "New Contact Us Message",
            $"From {request.Name}: {request.Subject}",
            NotificationType.CONTACT_US_MESSAGE
        );

        return Result.Succeed();
    }
}
