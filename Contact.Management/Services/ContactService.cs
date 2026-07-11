using Contact.Management.Contracts;
using Clinic.Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Contact.Management.Services;

public class ContactService(
    AppDbContext context,
    IEmailSender emailSender) : IContactService
{
    private readonly AppDbContext _context = context;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task<Result<List<ContactMessageResponse>>> GetAllAsync()
    {
        var messages = await _context.ContactUsMessages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageResponse(
                m.Id,
                m.Name,
                m.Email,
                m.Phone,
                m.Subject,
                m.Body,
                m.IsRead,
                m.CreatedAt
            ))
            .AsNoTracking()
            .ToListAsync();

        return Result.Succeed(messages);
    }

    public async Task<Result> MarkAsReadAsync(int contactId)
    {
        var message = await _context.ContactUsMessages
            .FirstOrDefaultAsync(m => m.Id == contactId);

        if (message is null)
            return Result.Failure(Error.NotFound("ContactMessage.NotFound", "Contact message not found"));

        message.IsRead = true;
        await _context.SaveChangesAsync();

        return Result.Succeed();
    }

    public async Task<Result> ReplyAsync(AdminContactReplyRequest request)
    {
        var message = await _context.ContactUsMessages
            .FirstOrDefaultAsync(m => m.Id == request.ContactId);

        if (message is null)
            return Result.Failure(Error.NotFound("ContactMessage.NotFound", "Contact message not found"));

        var htmlBody = EmailBodyBuilder.GenerateEmailBody("ContactReply", new Dictionary<string, string>
        {
            { "{{name}}", message.Name },
            { "{{subject}}", message.Subject },
            { "{{reply}}", request.Reply }
        });

        await _emailSender.SendEmailAsync(message.Email, $"Re: {message.Subject}", htmlBody);

        message.IsRead = true;
        await _context.SaveChangesAsync();

        return Result.Succeed();
    }
}
