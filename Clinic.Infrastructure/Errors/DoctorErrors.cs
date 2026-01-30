using Clinic.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Clinic.Infrastructure.Errors;

public static class DoctorErrors
{
    public static readonly Error DoctorNotFound = new("Doctor.NotFound", "Doctor not found", StatusCodes.Status404NotFound);
    public static readonly Error EmailDuplicated = new("Doctor.EmailDuplicated", "Email already exists", StatusCodes.Status409Conflict);
    public static readonly Error InvalidRoles = new("Doctor.InvalidRoles", "Invalid roles provided", StatusCodes.Status400BadRequest);
}
