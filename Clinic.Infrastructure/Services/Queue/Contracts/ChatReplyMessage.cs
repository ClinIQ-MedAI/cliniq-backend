using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Services.Queue.Contracts;

public class ChatReplyMessage
{
    [JsonPropertyName("chat_id")]
    public string ChatId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("reply")]
    public string? Reply { get; set; }

    [JsonPropertyName("query_type")]
    public string? QueryType { get; set; }

    [JsonPropertyName("show_upload")]
    public bool ShowUpload { get; set; }

    [JsonPropertyName("patient_id")]
    public string PatientId { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("worker")]
    public string? Worker { get; set; }

    [JsonPropertyName("duration_ms")]
    public double? DurationMs { get; set; }

    [JsonPropertyName("finished_at")]
    public string? FinishedAt { get; set; }
}
