namespace Clinic.Infrastructure.Settings;

public class QueueSettings
{
    public const string SectionName = "QueueSettings";

    public string QueueBackend { get; set; } = "redis"; // none, redis, or qstash
    public string QueuePrefix { get; set; } = "cliniq";
    public string QueueGroup { get; set; } = "backend";
    public string QueueResultChannel { get; set; } = "cliniq:results";
    public string ChatRequestChannel { get; set; } = "cliniq:chat:requests";
    public string ChatResultChannel { get; set; } = "cliniq:chat:results";
    public string ChatGroup { get; set; } = "backend";

    public string QstashUrl { get; set; } = "https://qstash.upstash.io";
    public string QstashToken { get; set; } = "";
    public string QstashCurrentSigningKey { get; set; } = "";
    public string QstashNextSigningKey { get; set; } = "";
    public string QstashCallbackUrl { get; set; } = "";
}
