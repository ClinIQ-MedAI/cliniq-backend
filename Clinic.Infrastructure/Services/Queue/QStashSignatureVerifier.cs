using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Clinic.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinic.Infrastructure.Services.Queue;

public class QStashSignatureVerifier : IQStashSignatureVerifier
{
    private readonly QueueSettings _settings;

    public QStashSignatureVerifier(IOptions<QueueSettings> settings)
    {
        _settings = settings.Value;
    }

    public bool Verify(string signature, string rawBody, string currentUrl)
    {
        var currentKey = _settings.QstashCurrentSigningKey;
        var nextKey = _settings.QstashNextSigningKey;

        if (string.IsNullOrEmpty(currentKey))
        {
            return false;
        }

        // Try current key, fallback to next key
        if (TryVerify(signature, rawBody, currentUrl, currentKey))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(nextKey) && TryVerify(signature, rawBody, currentUrl, nextKey))
        {
            return true;
        }

        return false;
    }

    private bool TryVerify(string token, string rawBody, string currentUrl, string signingKey)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateIssuer = true,
                ValidIssuer = "Upstash",
                ValidateAudience = false, // Validate 'sub' claim manually for proxy flexibility
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(60) // 1 minute tolerance
            };

            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            if (principal == null || validatedToken is not JwtSecurityToken jwtToken)
            {
                return false;
            }

            // Verify 'sub' claim matches currentUrl path to account for reverse proxies/ports
            var sub = jwtToken.Subject;
            if (string.IsNullOrEmpty(sub))
            {
                return false;
            }

            if (Uri.TryCreate(sub, UriKind.Absolute, out var subUri) && Uri.TryCreate(currentUrl, UriKind.Absolute, out var currentUri))
            {
                if (!subUri.AbsolutePath.Equals(currentUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            else
            {
                // Fallback to direct string check if parsing fails
                if (!sub.Contains(currentUrl) && !currentUrl.Contains(sub))
                {
                    return false;
                }
            }

            // Verify body claim matches computed SHA-256 hash of rawBody
            if (!jwtToken.Payload.TryGetValue("body", out var bodyClaimObj) || bodyClaimObj is not string bodyClaim)
            {
                return false;
            }

            using var sha256 = SHA256.Create();
            byte[] bodyBytes = Encoding.UTF8.GetBytes(rawBody);
            byte[] computedHashBytes = sha256.ComputeHash(bodyBytes);
            string computedHash = Base64UrlEncoder.Encode(computedHashBytes);

            string cleanComputed = computedHash.TrimEnd('=');
            string cleanClaim = bodyClaim.TrimEnd('=');

            return string.Equals(cleanComputed, cleanClaim, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}
