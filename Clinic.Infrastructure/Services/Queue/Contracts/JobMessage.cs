using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Services.Queue.Contracts;

public class JobMessage
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("modality")]
    public string Modality { get; set; } = string.Empty;

    [JsonPropertyName("image_base64")]
    public string? ImageBase64 { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("patient_id")]
    public string PatientId { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public object? Options { get; set; }

    [JsonPropertyName("reply_to")]
    public string? ReplyTo { get; set; }

    [JsonPropertyName("enqueued_at")]
    public string EnqueuedAt { get; set; } = string.Empty;
}
