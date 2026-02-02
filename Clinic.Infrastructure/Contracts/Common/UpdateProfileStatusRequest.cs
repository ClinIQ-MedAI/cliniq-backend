namespace Clinic.Infrastructure.Contracts.Common;

/// <summary>
/// Request to update profile status (Active/Suspended).
/// </summary>
public record UpdateProfileStatusRequest(bool Active);
