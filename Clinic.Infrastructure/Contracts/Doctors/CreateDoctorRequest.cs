using Clinic.Infrastructure.Abstractions.Enums;

namespace Clinic.Infrastructure.Contracts.Doctors;

public record CreateDoctorRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    string Specialization,
    string LicenseNumber
);
