namespace Clinic.Infrastructure.Entities;

public class DoctorAvailability
{
    public int Id { get; set; }
    public required string DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}