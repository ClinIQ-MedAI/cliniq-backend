using Booking.Patient.Contracts;

namespace Booking.Patient.Services;

public interface IHomeService
{
    Task<List<FlutterSpecializationDto>> GetSpecializationsAsync(CancellationToken cancellationToken = default);
    Task<List<FlutterSuggestedDoctorDto>> GetSuggestedDoctorsAsync(CancellationToken cancellationToken = default);
    Task<List<FlutterNewsDto>> GetNewsAsync(CancellationToken cancellationToken = default);
    Task<Result<FlutterProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default);
}
