using Clinic.Infrastructure.Abstractions;
using Contact.Management.Contracts;

namespace Contact.Management.Services;

public interface IContactManagementService
{
    Task<Result<List<ContactMessageResponse>>> GetAllAsync();
    Task<Result> MarkAsReadAsync(int contactId);
    Task<Result> ReplyAsync(AdminContactReplyRequest request);
}
