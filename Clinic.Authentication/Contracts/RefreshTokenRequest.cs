namespace Clinic.Authentication.Contracts;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);
