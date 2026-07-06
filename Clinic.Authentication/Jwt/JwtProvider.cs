using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Authentication.Jwt;

/// <summary>
/// JWT token provider implementation.
/// </summary>
public class JwtProvider(
    IOptions<JwtOptions> options,
    UserManager<ApplicationUser> userManager) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public (string Token, int ExpiresIn) GenerateToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        PatientStatus patientStatus,
        DoctorStatus doctorStatus)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Verification claims
            new("email_confirmed", user.EmailConfirmed.ToString().ToLower()),
            new("phone_number_confirmed", user.PhoneNumberConfirmed.ToString().ToLower()),
            // Status claims
            new("patient_status", patientStatus.ToString()),
            new("doctor_status", doctorStatus.ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim("roles", role));

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (Token: tokenString, ExpiresIn: _options.ExpiryMinutes * 60);
    }

    public async Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(
        ApplicationUser user,
        PatientStatus patientStatus,
        DoctorStatus doctorStatus)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var (token, expiresIn) = GenerateToken(
            user,
            roles,
            patientStatus,
            doctorStatus);
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

        return (token, expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = symmetricSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken.Claims.First(t => t.Type == JwtRegisteredClaimNames.Sub).Value;
        }
        catch
        {
            return null;
        }
    }
}
