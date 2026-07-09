using System.Text.Json.Serialization;

namespace Clinic.Infrastructure.Services.Queue.Contracts;

public class ResultMessage
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("modality")]
    public string Modality { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // completed or failed

    [JsonPropertyName("result")]
    public object? Result { get; set; } // predict_for_llm payload

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("patient_id")]
    public string PatientId { get; set; } = string.Empty;

    [JsonPropertyName("worker")]
    public string? Worker { get; set; }

    [JsonPropertyName("duration_ms")]
    public double DurationMs { get; set; }

    [JsonPropertyName("finished_at")]
    public string FinishedAt { get; set; } = string.Empty;
}
