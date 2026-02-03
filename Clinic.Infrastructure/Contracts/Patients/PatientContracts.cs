using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Contracts.Patients;

public record CreatePatientRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender
);

public record UpdatePatientRequest(
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    Gender? Gender = null
);

public record PatientResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsDisabled,
    PatientStatus Status
);

public record PatientProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateOnly? DateOfBirth,
    Gender? Gender
);
