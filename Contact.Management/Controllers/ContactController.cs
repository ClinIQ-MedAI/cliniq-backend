using Clinic.Authentication.Authorization;
using Contact.Management.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contact.Management.Controllers;

[ApiController]
[Route("admin/contact-us")]
[HasPermission(Permissions.ManageContacts)]
public class ContactController(
    IContactService contactService) : ControllerBase
{
    private readonly IContactService _contactService = contactService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _contactService.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _contactService.MarkAsReadAsync(id);
        return result.IsSucceed ? NoContent() : result.ToProblem();
    }
}
