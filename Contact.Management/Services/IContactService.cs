using Clinic.Infrastructure.Abstractions;
using Contact.Management.Contracts;

namespace Contact.Management.Services;

public interface IContactService
{
    Task<Result<List<ContactMessageResponse>>> GetAllAsync();
    Task<Result> MarkAsReadAsync(int contactId);
}
