using Clinic.Infrastructure.Entities.Enums;

namespace Clinic.Infrastructure.Entities;

public class Booking
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public PatientProfile Patient { get; set; } = null!;
    public int DoctorScheduleId { get; set; }
    public DoctorSchedule DoctorSchedule { get; set; } = null!;
    public BookingStatus Status { get; set; } = BookingStatus.PENDING;
}