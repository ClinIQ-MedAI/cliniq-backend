using Clinic.AIFeatures.Contracts;
using Clinic.AIFeatures.Services;
using Clinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.AIFeatures.Controllers;

[Authorize]
[ApiController]
[Route("chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatbotRequest request, CancellationToken ct)
    {
        var patientId = User.GetUserId()!;
        var result = await _chatbotService.SendMessageAsync(patientId, request, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var patientId = User.GetUserId()!;
        var result = await _chatbotService.GetChatHistoryAsync(patientId, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }
}
