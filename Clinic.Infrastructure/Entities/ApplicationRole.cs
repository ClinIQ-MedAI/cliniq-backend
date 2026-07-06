using Microsoft.AspNetCore.Identity;

namespace Clinic.Infrastructure.Entities;

public class ApplicationRole : IdentityRole
{
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }
}
