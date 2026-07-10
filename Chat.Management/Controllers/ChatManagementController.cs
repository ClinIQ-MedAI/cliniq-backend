using Chat.Management.Services;
using Clinic.Authentication.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Management.Controllers;

[ApiController]
[Route("admin/chat")]
public class ChatManagementController(IChatService chatService) : ControllerBase
{
    private readonly IChatService _chatService = chatService;

    [HttpGet("conversations")]
    [HasPermission(Permissions.GetChats)]
    public async Task<IActionResult> GetAllConversations(CancellationToken ct)
    {
        var result = await _chatService.GetAllConversationsAsync(ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("conversations/{conversationId:int}/messages")]
    [HasPermission(Permissions.GetChats)]
    public async Task<IActionResult> GetMessages(int conversationId, CancellationToken ct)
    {
        var result = await _chatService.GetMessagesAsync(conversationId, ct);
        return result.IsSucceed ? Ok(result.Value) : result.ToProblem();
    }
}
