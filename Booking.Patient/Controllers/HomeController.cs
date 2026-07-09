using Booking.Patient.Contracts;
using Booking.Patient.Services;

namespace Booking.Patient.Controllers;

[ApiController]
[Route("patient/home")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations(CancellationToken cancellationToken)
    {
        var specializations = await _homeService.GetSpecializationsAsync(cancellationToken);
        return Ok(new ApiResponse<List<FlutterSpecializationDto>>(true, "Specializations fetched successfully", specializations));
    }

    [HttpGet("suggested-doctors")]
    public async Task<IActionResult> GetSuggestedDoctors(CancellationToken cancellationToken)
    {
        var doctors = await _homeService.GetSuggestedDoctorsAsync(cancellationToken);
        return Ok(new ApiResponse<List<FlutterSuggestedDoctorDto>>(true, "Doctors fetched successfully", doctors));
    }

    [HttpGet("news")]
    public async Task<IActionResult> GetNews(CancellationToken cancellationToken)
    {
        var news = await _homeService.GetNewsAsync(cancellationToken);
        return Ok(new ApiResponse<List<FlutterNewsDto>>(true, "News fetched successfully", news));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _homeService.GetMyProfileAsync(userId!, cancellationToken);

        if (result.IsFailure)
            return NotFound(new ApiResponse<object>(false, result.Error.Description, null));

        return Ok(new ApiResponse<FlutterProfileResponse>(true, "Profile fetched successfully", result.Value));
    }
}
