using Clinic.Infrastructure.Abstractions;
using Notification.Management.Contracts;

namespace Notification.Management.Services;

public class NotificationService(
    AppDbContext context,
    global::Clinic.Infrastructure.Services.INotificationService notificationService) : INotificationService
{
    private readonly AppDbContext _context = context;
    private readonly global::Clinic.Infrastructure.Services.INotificationService _notificationService = notificationService;

    public async Task<Result> SendToUsersAsync(SendNotificationRequest request)
    {
        List<string> userIds;

        switch (request.UserType)
        {
            case UserTypeTarget.Patients:
                userIds = await _context.PatientProfiles
                    .Select(p => p.Id)
                    .ToListAsync();
                break;
            case UserTypeTarget.Doctors:
                userIds = await _context.DoctorProfiles
                    .Select(d => d.Id)
                    .ToListAsync();
                break;
            default:
                var patientIds = await _context.PatientProfiles.Select(p => p.Id).ToListAsync();
                var doctorIds = await _context.DoctorProfiles.Select(d => d.Id).ToListAsync();
                userIds = patientIds.Concat(doctorIds).Distinct().ToList();
                break;
        }

        await _notificationService.CreateNotificationAsync(
            request.Title,
            request.Body,
            NotificationType.ADMIN_BROADCAST,
            userIds
        );

        return Result.Succeed();
    }
}
