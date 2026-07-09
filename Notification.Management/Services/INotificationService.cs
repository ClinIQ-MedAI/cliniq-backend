using Clinic.Infrastructure.Abstractions;
using Notification.Management.Contracts;

namespace Notification.Management.Services;

public interface INotificationService
{
    Task<Result> SendToUsersAsync(SendNotificationRequest request);
}
