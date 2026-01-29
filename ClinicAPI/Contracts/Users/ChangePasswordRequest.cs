namespace ClinicAPI.Contracts.Users;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);