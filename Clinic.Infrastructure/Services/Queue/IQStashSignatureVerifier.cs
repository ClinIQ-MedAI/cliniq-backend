namespace Clinic.Infrastructure.Services.Queue;

public interface IQStashSignatureVerifier
{
    bool Verify(string signature, string rawBody, string currentUrl);
}
