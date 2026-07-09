using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Clinic.Infrastructure.Services.Queue.Contracts;
using Clinic.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Clinic.Infrastructure.Services.Queue;

public class AIServiceClient : IAIServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly AIServiceSettings _settings;

    public AIServiceClient(HttpClient httpClient, IOptions<AIServiceSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<ResultMessage> SendPredictRequestAsync(string modality, JobMessage request, CancellationToken cancellationToken = default)
    {
        var serviceUrl = _settings.GetServiceUrl(modality);
        var endpoint = $"{serviceUrl.TrimEnd('/')}/predict_for_llm";

        var startTime = DateTime.UtcNow;
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
            var finishedTime = DateTime.UtcNow;
            var durationMs = (finishedTime - startTime).TotalMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // The endpoint returns the result payload directly
                var resultObj = JsonSerializer.Deserialize<object>(responseContent);
                return new ResultMessage
                {
                    JobId = request.JobId,
                    Modality = modality,
                    Status = "completed",
                    Result = resultObj,
                    PatientId = request.PatientId,
                    Worker = $"http:{modality}",
                    DurationMs = durationMs,
                    FinishedAt = finishedTime.ToString("o")
                };
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ResultMessage
                {
                    JobId = request.JobId,
                    Modality = modality,
                    Status = "failed",
                    Error = $"HTTP request failed with status {response.StatusCode}. Details: {errorText}",
                    PatientId = request.PatientId,
                    Worker = $"http:{modality}",
                    DurationMs = durationMs,
                    FinishedAt = DateTime.UtcNow.ToString("o")
                };
            }
        }
        catch (Exception ex)
        {
            var finishedTime = DateTime.UtcNow;
            var durationMs = (finishedTime - startTime).TotalMilliseconds;
            return new ResultMessage
            {
                JobId = request.JobId,
                Modality = modality,
                Status = "failed",
                Error = ex.Message,
                PatientId = request.PatientId,
                Worker = $"http:{modality}",
                DurationMs = durationMs,
                FinishedAt = finishedTime.ToString("o")
            };
        }
    }
}
