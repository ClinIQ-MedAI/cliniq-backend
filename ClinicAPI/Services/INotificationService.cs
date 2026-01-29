namespace ClinicAPI.Services;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
}
