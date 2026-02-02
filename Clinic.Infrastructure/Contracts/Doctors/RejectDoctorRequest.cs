namespace Clinic.Infrastructure.Contracts.Doctors;

/// <summary>
/// Request to reject a doctor profile application.
/// </summary>
/// <param name="Reason">The reason for rejection.</param>
public record RejectDoctorRequest(string Reason);
