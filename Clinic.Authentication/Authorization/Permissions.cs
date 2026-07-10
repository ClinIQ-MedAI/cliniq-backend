namespace Clinic.Authentication.Authorization;

public static class Permissions
{
    public const string GetPatients = "Permissions.Patients.View";
    public const string AddPatients = "Permissions.Patients.Create";
    public const string UpdatePatients = "Permissions.Patients.Update";
    public const string DeletePatients = "Permissions.Patients.Delete";

    public const string GetDoctors = "Permissions.Doctors.View";
    public const string AddDoctors = "Permissions.Doctors.Create";
    public const string UpdateDoctors = "Permissions.Doctors.Update";
    public const string DeleteDoctors = "Permissions.Doctors.Delete";

    public const string ViewRoles = "Permissions.Roles.View";
    public const string CreateRoles = "Permissions.Roles.Create";
    public const string UpdateRoles = "Permissions.Roles.Update";
    public const string DeleteRoles = "Permissions.Roles.Delete";

    public const string GetBookings = "Permissions.Bookings.View";
    public const string UpdateBookings = "Permissions.Bookings.Update";

    public const string GetChats = "Permissions.Chats.View";

    public const string SendNotifications = "Permissions.Notifications.Send";

    public const string ViewDashboard = "Permissions.Dashboard.View";

    public const string ManageContacts = "Permissions.Contacts.Manage";
}
