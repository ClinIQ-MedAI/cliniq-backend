namespace Clinic.Infrastructure.Settings;

public class AIServiceSettings
{
    public const string SectionName = "AIServiceSettings";

    public string BoneUrl { get; set; } = "http://localhost:8001";
    public string DentalXrayUrl { get; set; } = "http://localhost:8002";
    public string ChestUrl { get; set; } = "http://localhost:8003";
    public string DentalPhotoUrl { get; set; } = "http://localhost:8004";
    public string PrescriptionUrl { get; set; } = "http://localhost:8005";

    public string GetServiceUrl(string modality)
    {
        return modality.ToLowerInvariant() switch
        {
            "bone" => BoneUrl,
            "dental_xray" => DentalXrayUrl,
            "chest" => ChestUrl,
            "dental_photo" => DentalPhotoUrl,
            "prescription" => PrescriptionUrl,
            _ => throw new ArgumentException($"Unknown modality: {modality}")
        };
    }
}
