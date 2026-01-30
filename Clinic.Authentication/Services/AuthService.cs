using Clinic.Authentication.Contracts;
using Clinic.Authentication.Jwt;
using Clinic.Authentication.Strategies;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authentication.Services;

/// <summary>
/// Authentication service that handles login via password or OTP.
/// </summary>
public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    IJwtProvider jwtProvider,
    IEnumerable<ILoginStrategy> loginStrategies) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AppDbContext _context = context;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IEnumerable<ILoginStrategy> _loginStrategies = loginStrategies;

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return AuthResult.Failure("Invalid email or password");

        // Check if user is verified
        if (!user.EmailVerified && !user.PhoneVerified)
            return AuthResult.Failure("Please verify your email or phone before logging in");

        // Select appropriate login strategy
        ILoginStrategy? strategy = _loginStrategies.FirstOrDefault(s => s.CanHandle(request));
        if (strategy == null)
            return AuthResult.Failure("Invalid login request - provide password or OTP");

        // Validate credentials using strategy
        var isValid = await strategy.ValidateAsync(user, request, cancellationToken);
        if (!isValid)
            return AuthResult.Failure("Invalid credentials");

        // Load profile statuses
        var patientProfile = await _context.PatientProfiles
            .Where(p => p.Id == user.Id)
            .Select(p => new { p.Status })
            .FirstOrDefaultAsync(cancellationToken);

        var doctorProfile = await _context.DoctorProfiles
            .Where(d => d.Id == user.Id)
            .Select(d => new { d.Status })
            .FirstOrDefaultAsync(cancellationToken);

        var patientStatus = patientProfile?.Status ?? PatientStatus.INCOMPLETE_PROFILE;
        var doctorStatus = doctorProfile?.Status ?? DoctorStatus.INCOMPLETE_PROFILE;

        // Generate JWT token
        var (token, expiresAt) = await _jwtProvider.GenerateTokenAsync(user, patientStatus, doctorStatus);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        return AuthResult.Success(token, refreshToken, expiresAt, patientStatus, doctorStatus);
    }
}
