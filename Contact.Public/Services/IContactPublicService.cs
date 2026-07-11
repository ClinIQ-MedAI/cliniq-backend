using Clinic.Infrastructure.Abstractions;
using Contact.Public.Contracts;

namespace Contact.Public.Services;

public interface IContactPublicService
{
    Task<Result> SubmitAsync(ContactUsRequest request);
}
