using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Contracts.Doctors;

public record DoctorResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsDisabled,
    DoctorStatus Status
);
