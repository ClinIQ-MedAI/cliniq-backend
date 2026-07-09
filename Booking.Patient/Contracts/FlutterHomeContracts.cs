namespace Booking.Patient.Contracts;

public record FlutterProfileResponse(
    string Id,
    string Name,
    string Email,
    string Role
);

public record FlutterSpecializationDto(
    string Id,
    string Name,
    string Image
);

public record FlutterSuggestedDoctorDto(
    string Id,
    string Name,
    string Image,
    string Speciality,
    string Experience,
    string Rating,
    string NumberOfAppointments,
    string City
);

public record FlutterNewsDto(
    string Id,
    string Title,
    string Image,
    string Description
);
