using Booking.Patient.Contracts;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Booking.Patient.Services;

public class BookingPatientHomeService : IBookingPatientHomeService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingPatientHomeService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    private static readonly Dictionary<string, string> SpecializationImages = new()
    {
        ["Cardiology"] = "https://img.freepik.com/free-vector/heart-attack-concept-illustration_114360-1014.jpg",
        ["Dermatology"] = "https://img.freepik.com/free-vector/skin-layer-diagram-medical-educational-poster_1308-59648.jpg",
        ["Neurology"] = "https://img.freepik.com/free-vector/brain-anatomy-concept-illustration_114360-1049.jpg",
        ["Pediatrics"] = "https://img.freepik.com/free-vector/hand-drawn-pediatrician-concept_23-2148492044.jpg",
        ["Orthopedics"] = "https://img.freepik.com/free-vector/orthopedics-rehabilitation-center-flat-composition-with-physicians-helping-patients-orthopedic-devices_1284-59695.jpg",
        ["Dentistry"] = "https://img.freepik.com/free-vector/dentist-with-patient-concept-illustration_114360-4493.jpg",
        ["Psychiatrist"] = "https://img.freepik.com/free-vector/mental-health-concept-illustration_114360-1014.jpg",
        ["Oncology"] = "https://img.freepik.com/free-vector/cancer-research-concept-illustration_114360-1049.jpg",
        ["Gynecologist"] = "https://img.freepik.com/free-vector/pregnancy-concept-illustration_114360-1014.jpg",
        ["Opthalmologist"] = "https://img.freepik.com/free-vector/eye-anatomy-concept-illustration_114360-1014.jpg",
        ["Gastroenterologist"] = "https://img.freepik.com/free-vector/digestive-system-concept-illustration_114360-1014.jpg",
        ["Endocrinologist"] = "https://img.freepik.com/free-vector/hormones-concept-illustration_114360-1014.jpg",
        ["Pulmonologist"] = "https://img.freepik.com/free-vector/lungs-anatomy-concept-illustration_114360-1014.jpg",
        ["Rheumatologist"] = "https://img.freepik.com/free-vector/joint-pain-concept-illustration_114360-1014.jpg",
        ["Nephrologist"] = "https://img.freepik.com/free-vector/kidney-anatomy-concept-illustration_114360-1014.jpg",
        ["General Surgeon"] = "https://img.freepik.com/free-vector/surgery-concept-illustration_114360-1014.jpg",
        ["Urology"] = "https://img.freepik.com/free-vector/urinary-system-concept-illustration_114360-1014.jpg",
    };

    private static readonly string DefaultSpecImage =
        "https://img.freepik.com/free-vector/doctor-with-medical-icons_114360-1014.jpg";

    public async Task<List<FlutterSpecializationDto>> GetSpecializationsAsync(CancellationToken cancellationToken = default)
    {
        var specializations = await _dbContext.DoctorProfiles
            .Where(d => d.Status == DoctorStatus.ACTIVE && d.Specialization != null)
            .Select(d => d.Specialization!)
            .Distinct()
            .OrderBy(s => s)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        int id = 1;
        return specializations.Select(s => new FlutterSpecializationDto(
            (id++).ToString(),
            s,
            SpecializationImages.GetValueOrDefault(s, DefaultSpecImage)
        )).ToList();
    }

    public async Task<List<FlutterSuggestedDoctorDto>> GetSuggestedDoctorsAsync(CancellationToken cancellationToken = default)
    {
        var doctors = await _dbContext.DoctorProfiles
            .Include(d => d.User)
            .Where(d => d.Status == DoctorStatus.ACTIVE)
            .OrderBy(d => d.User.FirstName)
            .Take(10)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var doctorIds = doctors.Select(d => d.Id).ToList();

        var appointmentCounts = await _dbContext.Bookings
            .Where(b => doctorIds.Contains(b.DoctorSchedule.DoctorId) && b.Status != BookingStatus.CANCELLED)
            .GroupBy(b => b.DoctorSchedule.DoctorId)
            .Select(g => new { DoctorId = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var countMap = appointmentCounts.ToDictionary(x => x.DoctorId, x => x.Count);

        return doctors.Select(d => new FlutterSuggestedDoctorDto(
            d.Id,
            $"Dr. {d.User.FirstName} {d.User.LastName}",
            d.PersonalIdentityPhotoUrl ?? "",
            d.Specialization ?? "General",
            "10 years",
            "4.5",
            (countMap.GetValueOrDefault(d.Id, 0)).ToString(),
            "Cairo"
        )).ToList();
    }

    public async Task<List<FlutterNewsDto>> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        var news = await _dbContext.HealthNews
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return news.Select(n => new FlutterNewsDto(
            n.Id.ToString(),
            n.Title,
            n.Image,
            n.Description
        )).ToList();
    }

    public async Task<Result<FlutterProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<FlutterProfileResponse>(Error.NotFound("User.NotFound", "User not found"));

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";
        var roleMapped = role switch
        {
            "Patient" => "customer",
            "Doctor" => "doctor",
            "Admin" or "SuperAdmin" => "admin",
            _ => role
        };

        return Result.Succeed(new FlutterProfileResponse(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email ?? "",
            roleMapped
        ));
    }
}
