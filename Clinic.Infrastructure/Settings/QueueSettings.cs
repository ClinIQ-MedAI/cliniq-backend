namespace Clinic.Infrastructure.Settings;

public class QueueSettings
{
    public const string SectionName = "QueueSettings";

    public string QueueBackend { get; set; } = "none"; // none or redis
    public string QueuePrefix { get; set; } = "cliniq";
    public string QueueGroup { get; set; } = "backend";
    public string QueueResultChannel { get; set; } = "cliniq:results";
}
