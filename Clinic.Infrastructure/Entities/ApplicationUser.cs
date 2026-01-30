using Microsoft.AspNetCore.Identity;
using Clinic.Infrastructure.Abstractions;

using Clinic.Infrastructure.Abstractions.Enums;

namespace Clinic.Infrastructure.Entities;

/// <summary>
/// Unified ApplicationUser for all user types (Doctor, Patient, Admin).
/// Each user can have zero or more profile types.
/// </summary>
public class ApplicationUser : IdentityUser, IApplicationUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }

    // Registration fields
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }

    // Navigation properties for profile types (1-to-0..1 relationships)
    public DoctorProfile? DoctorProfile { get; set; }
    public PatientProfile? PatientProfile { get; set; }

    // Refresh tokens for JWT auth
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
