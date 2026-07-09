using Contact.Management.Contracts;

namespace Contact.Management.Services;

public class ContactService(
    AppDbContext context) : IContactService
{
    private readonly AppDbContext _context = context;

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
}
