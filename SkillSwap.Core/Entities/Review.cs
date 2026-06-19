namespace SkillSwap.Core.Entities;

public class Review
{
    public int Id { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public int SwapRequestId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser Reviewer { get; set; } = null!;
    public AppUser Reviewee { get; set; } = null!;
    public SwapRequest SwapRequest { get; set; } = null!;
}
