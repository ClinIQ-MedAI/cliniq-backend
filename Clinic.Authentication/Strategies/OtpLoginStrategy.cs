using Clinic.Authentication.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authentication.Strategies;

/// <summary>
/// Login strategy using OTP code.
/// </summary>
public class OtpLoginStrategy(AppDbContext context) : ILoginStrategy
{
    private readonly AppDbContext _context = context;

    public bool CanHandle(LoginRequest request)
    {
        return !string.IsNullOrEmpty(request.OtpCode);
    }

    public async Task<bool> ValidateAsync(ApplicationUser user, LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.OtpCode))
            return false;

        // Find valid OTP for this user
        var otp = await _context.Set<OtpCode>()
            .Where(o => o.UserId == user.Id && o.Code == request.OtpCode && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null)
            return false;

        // Mark OTP as used
        otp.IsUsed = true;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
