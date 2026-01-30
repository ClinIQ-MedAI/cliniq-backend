namespace Clinic.Infrastructure.Authentication;

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
}
