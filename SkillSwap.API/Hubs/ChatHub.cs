using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SkillSwap.Core.DTOs.Messages;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService) => _messageService = messageService;

    public async Task SendMessage(string receiverId, string content)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User not authenticated.");

        var dto = new SendMessageDto { ReceiverId = receiverId, Content = content };
        var message = await _messageService.SendMessageAsync(senderId, dto);

        await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
        await Clients.Caller.SendAsync("ReceiveMessage", message);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }
}
