using Contact.Public.Contracts;
using Contact.Public.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contact.Public.Controllers;

[ApiController]
[Route("contact-us")]
public class ContactController(
    IContactService contactService) : ControllerBase
{
    private readonly IContactService _contactService = contactService;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactUsRequest request)
    {
        var result = await _contactService.SubmitAsync(request);
        return result.IsSucceed ? Ok() : result.ToProblem();
    }
}
