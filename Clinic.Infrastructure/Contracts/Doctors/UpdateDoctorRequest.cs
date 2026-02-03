using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;

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
