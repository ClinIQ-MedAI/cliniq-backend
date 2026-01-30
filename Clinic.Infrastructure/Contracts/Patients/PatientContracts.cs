namespace Clinic.Infrastructure.Contracts.Patients;

public record CreatePatientRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);

public record UpdatePatientRequest(
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);

public record PatientResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsDisabled,
    IEnumerable<string> Roles
);

public record PatientProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateOnly DateOfBirth, // Assuming Profile has extra fields
    string Gender
);
