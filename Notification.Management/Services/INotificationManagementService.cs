using Clinic.Infrastructure.Abstractions;
using Notification.Management.Contracts;

namespace Notification.Management.Services;

public interface INotificationManagementService
{
    Task<Result> SendToUsersAsync(SendNotificationRequest request);
}
