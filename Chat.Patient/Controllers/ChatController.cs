using Chat.Patient.Contracts;
using Chat.Patient.Localization;
using Chat.Patient.Services;
using Clinic.Authentication.Authorization;
using Clinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Chat.Patient.Controllers;

[ApiController]
[Route("chat")]
[Authorize(Policy = PolicyNames.ActivePatient)]
public class ChatController(
    IChatService chatService,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    private readonly IChatService _chatService = chatService;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var patientId = User.GetUserId()!;
        var result = await _chatService.GetConversationsAsync(patientId, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("conversations/{conversationId:int}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId, CancellationToken ct)
    {
        var patientId = User.GetUserId()!;
        var result = await _chatService.GetMessagesAsync(patientId, conversationId, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken ct)
    {
        var patientId = User.GetUserId()!;
        var result = await _chatService.StartConversationAsync(patientId, request, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("conversations/{conversationId:int}/messages")]
    public async Task<IActionResult> SendMessage(int conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var patientId = User.GetUserId()!;
        var result = await _chatService.SendMessageAsync(patientId, conversationId, request, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }
}
