namespace Clinic.Authentication.Contracts;

public record ConfirmEmailRequest(
    string UserId,
    string Code
);
