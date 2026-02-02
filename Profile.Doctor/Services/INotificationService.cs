namespace Profile.Doctor.Services;

public interface INotificationService
{
    Task SendNewPollsNotification(int? pollId = null);
}
