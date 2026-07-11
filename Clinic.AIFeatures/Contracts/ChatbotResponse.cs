namespace Clinic.AIFeatures.Contracts;

public class ChatbotResponse
{
    public int Id { get; set; }
    public string ChatId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string LanguagePreference { get; set; } = "ar";
    public string Status { get; set; } = string.Empty;
    public string? Reply { get; set; }
    public string? QueryType { get; set; }
    public bool ShowUpload { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
