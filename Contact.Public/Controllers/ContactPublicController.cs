using Contact.Public.Contracts;
using Contact.Public.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contact.Public.Controllers;

[ApiController]
[Route("contact-us")]
public class ContactPublicController(
    IContactPublicService contactService) : ControllerBase
{
    private readonly IContactPublicService _contactService = contactService;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactUsRequest request)
    {
        var result = await _contactService.SubmitAsync(request);
        return result.IsSucceed ? Ok() : result.ToProblem();
    }
}
