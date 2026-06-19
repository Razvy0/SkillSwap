namespace SkillSwap.Core.DTOs.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}
