using Chat.Doctor.Contracts;
using Chat.Doctor.Localization;
using Chat.Doctor.Services;
using Clinic.Authentication.Authorization;
using Clinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Chat.Doctor.Controllers;

[ApiController]
[Route("chat")]
[Authorize(Policy = PolicyNames.ActiveDoctor)]
public class ChatController(
    IChatService chatService,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    private readonly IChatService _chatService = chatService;
    private readonly IStringLocalizer<Messages> _localizer = localizer;

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var doctorId = User.GetUserId()!;
        var result = await _chatService.GetConversationsAsync(doctorId, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("conversations/{conversationId:int}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId, CancellationToken ct)
    {
        var doctorId = User.GetUserId()!;
        var result = await _chatService.GetMessagesAsync(doctorId, conversationId, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("conversations/{conversationId:int}/messages")]
    public async Task<IActionResult> SendMessage(int conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var doctorId = User.GetUserId()!;
        var result = await _chatService.SendMessageAsync(doctorId, conversationId, request, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }
}
