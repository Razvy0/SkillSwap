namespace SkillSwap.Core.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public int SwapRequestId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
