using Booking.Patient.Contracts;

namespace Booking.Patient.Services;

public interface IBookingPatientService
{
    Task<Result<int>> BookAppointmentAsync(string patientId, string doctorId, DateOnly date, CancellationToken cancellationToken = default);
    Task<List<BookingResponse>> GetMyBookingsAsync(string patientId, CancellationToken cancellationToken = default);
    Task<List<DoctorScheduleDto>> GetDoctorSchedulesAsync(string doctorId, CancellationToken cancellationToken = default);
    Task<Result<BookingScreenViewModel>> GetBookingScreenDetailsAsync(string doctorId, CancellationToken cancellationToken = default);
    Task<Result<List<DoctorSearchDto>>> GetAvailableDoctorsAsync(DateOnly date, CancellationToken cancellationToken = default);

    // Flutter-specific endpoints
    Task<List<FlutterAppointmentDto>> GetExaminationAppointmentsAsync(string patientId, CancellationToken cancellationToken = default);
    Task<List<FlutterAppointmentDto>> GetAvailableDoctorsFlutterAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<Result<FlutterDoctorScheduleDataDto>> GetDoctorScheduleFlutterAsync(string doctorId, CancellationToken cancellationToken = default);
    Task<Result<FlutterDoctorDetailsDataDto>> GetDoctorDetailsFlutterAsync(string doctorId, CancellationToken cancellationToken = default);
}
