namespace Clinic.Authentication.Jwt;

/// <summary>
/// JWT configuration options.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "JWT";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
