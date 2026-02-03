namespace Clinic.Infrastructure.Entities;

public class DoctorSchedule
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public DoctorProfile Doctor { get; set; } = null!;
    public DateOnly Date { get; set; }
    public int BookingCount { get; set; }
    public bool IsAvailable { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
}