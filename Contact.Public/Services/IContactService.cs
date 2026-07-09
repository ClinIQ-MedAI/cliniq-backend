using Clinic.Infrastructure.Abstractions;
using Contact.Public.Contracts;

namespace Contact.Public.Services;

public interface IContactService
{
    Task<Result> SubmitAsync(ContactUsRequest request);
}
