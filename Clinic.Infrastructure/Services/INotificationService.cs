namespace Clinic.Infrastructure.Services;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
}
