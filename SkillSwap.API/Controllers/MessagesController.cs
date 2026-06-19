using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Messages;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService) => _messageService = messageService;

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var conversations = await _messageService.GetConversationPartnersAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var count = await _messageService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    [HttpGet("{otherUserId}")]
    public async Task<IActionResult> GetConversation(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var messages = await _messageService.GetConversationAsync(userId, otherUserId);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var message = await _messageService.SendMessageAsync(userId, dto);
        return Ok(message);
    }

    [HttpPut("read/{senderId}")]
    public async Task<IActionResult> MarkAsRead(string senderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _messageService.MarkAsReadAsync(senderId, userId);
        return NoContent();
    }
}
