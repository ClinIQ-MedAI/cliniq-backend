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
}
