using Clinic.Infrastructure.Abstractions.Enums;
using Clinic.Infrastructure.Entities;

namespace Clinic.Infrastructure.Contracts.Doctors;

public record UpdateDoctorRequest(
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    Gender? Gender = null,
    string? Specialization = null,
    string? LicenseNumber = null,
    DoctorStatus? Status = null
);
