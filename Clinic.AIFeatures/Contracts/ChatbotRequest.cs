using System.ComponentModel.DataAnnotations;

namespace Clinic.AIFeatures.Contracts;

public class ChatbotRequest
{
    [Required]
    public string Message { get; set; } = string.Empty;

    public string LanguagePreference { get; set; } = "ar";
}
