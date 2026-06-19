using SkillSwap.Core.DTOs.Messages;

namespace SkillSwap.Core.Interfaces.Services;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetConversationAsync(string userId, string otherUserId);
    Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto);
    Task MarkAsReadAsync(string senderId, string receiverId);
    Task<IEnumerable<ConversationDto>> GetConversationPartnersAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
