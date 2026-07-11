namespace Clinic.Infrastructure.Contracts.Doctors;

public record DoctorBasicInfoResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email
);
