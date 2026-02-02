using Microsoft.AspNetCore.Identity.UI.Services;
using Clinic.Infrastructure.Helpers;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Clinic.Infrastructure.Services;

public class NotificationService(
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor) : INotificationService
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task SendNewPollsNotification(int? pollId = null)
    {
        // TODO: Implement poll notification when Poll entity is added to the ERD
        // This service will need Poll entity and corresponding DbSet in AppDbContext

        // For now, send a test notification to all confirmed users
        var users = await _userManager.Users
            .Where(u => u.EmailConfirmed)
            .ToListAsync();

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var user in users)
        {
            var placeHolders = new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName }
            };

            var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeHolders);

            await _emailSender.SendEmailAsync(user.Email!, "📋 Clinic API: Notification", body);
        }
    }
}
