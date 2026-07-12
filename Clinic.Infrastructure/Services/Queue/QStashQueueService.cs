using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Clinic.Infrastructure.Services.Queue;

public class QStashQueueService : IQueueService
{
    private readonly HttpClient _httpClient;
    private readonly QueueSettings _settings;
    private readonly AIServiceSettings _aiSettings;

    public QStashQueueService(
        HttpClient httpClient,
        IOptions<QueueSettings> settings,
        IOptions<AIServiceSettings> aiSettings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _aiSettings = aiSettings.Value;
    }

    public bool IsEnabled => true;

    public async Task PublishJobAsync(AIJob job)
    {
        var jobMessage = new JobMessage
        {
            JobId = job.Id,
            Modality = job.Modality.ToString().ToLowerInvariant(),
            ImageBase64 = job.ImageBase64,
            ImageUrl = job.ImageUrl,
            PatientId = job.PatientId,
            Options = job.OptionsJson != null ? JsonSerializer.Deserialize<object>(job.OptionsJson) : null,
            ReplyTo = job.ReplyTo,
            EnqueuedAt = job.CreatedAt.ToString("o")
        };

        var json = JsonSerializer.Serialize(jobMessage);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var modalityUrl = _aiSettings.GetServiceUrl(job.Modality.ToString());
        var destinationUrl = $"{modalityUrl.TrimEnd('/')}/predict_for_llm";
        var publishUrl = $"{_settings.QstashUrl.TrimEnd('/')}/v2/publish/{destinationUrl}";

        var request = new HttpRequestMessage(HttpMethod.Post, publishUrl);
        
        var token = _settings.QstashToken;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Upstash-Callback", $"{_settings.QstashCallbackUrl.TrimEnd('/')}/ai/jobs/callback/job");
        request.Headers.Add("Upstash-Retries", "3");
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task PublishChatAsync(string chatId, string patientId, string message, string languagePreference)
    {
        var payload = new
        {
            chat_id = chatId,
            message = message,
            patient_id = patientId,
            language_preference = languagePreference,
            enqueued_at = DateTime.UtcNow.ToString("o")
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var chatbotUrl = _aiSettings.ChatbotUrl;
        var destinationUrl = $"{chatbotUrl.TrimEnd('/')}/chat";
        var publishUrl = $"{_settings.QstashUrl.TrimEnd('/')}/v2/publish/{destinationUrl}";

        var request = new HttpRequestMessage(HttpMethod.Post, publishUrl);
        
        var token = _settings.QstashToken;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Upstash-Callback", $"{_settings.QstashCallbackUrl.TrimEnd('/')}/ai/jobs/callback/chat");
        request.Headers.Add("Upstash-Retries", "3");
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
