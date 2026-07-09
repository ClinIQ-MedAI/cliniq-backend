namespace Booking.Patient.Contracts;

public record ApiResponse<T>(bool Success, string Message, T? Data);

public record FlutterAppointmentDto(
    string Id,
    string DoctorName,
    string DoctorSpeciality,
    string DoctorImage,
    string AppointmentDate,
    string AppointmentTime,
    string AppointmentStatus
);

public record FlutterWeeklyScheduleDto(string Day, string Range);

public record FlutterScheduleDateDto(
    string Day,
    string Date,
    string Month,
    string FullDate,
    string PatientCount,
    bool IsFull
);

public record FlutterDoctorScheduleDataDto(
    List<FlutterWeeklyScheduleDto> WeeklySchedule,
    List<FlutterScheduleDateDto> Dates
);

public record FlutterDoctorDetailsDto(
    string Id,
    string Name,
    string Image,
    string Speciality,
    string Experience,
    string Rating,
    string NumberOfAppointments,
    string City
);

public record FlutterDoctorDetailsDataDto(
    FlutterDoctorDetailsDto Doctor,
    FlutterDoctorScheduleDataDto Schedule
);
