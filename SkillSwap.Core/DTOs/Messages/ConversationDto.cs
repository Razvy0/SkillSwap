namespace SkillSwap.Core.DTOs.Messages;

public class ConversationDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTimestamp { get; set; }
    public int UnreadCount { get; set; }
}
