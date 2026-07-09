namespace Notification.Management.Contracts;

public enum UserTypeTarget
{
    Patients,
    Doctors,
    All
}

public record SendNotificationRequest(
    string Title,
    string Body,
    UserTypeTarget UserType
);
