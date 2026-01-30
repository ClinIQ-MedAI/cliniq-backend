namespace Clinic.Infrastructure.Contracts.Doctors;

public record DoctorProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email
);
