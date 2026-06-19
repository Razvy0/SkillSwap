using SkillSwap.Core.Enums;

namespace SkillSwap.Core.Entities;

public class Dispute
{
    public int Id { get; set; }
    public int SwapRequestId { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string ReportedUserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; } = DisputeStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? AdminNotes { get; set; }

    public SwapRequest SwapRequest { get; set; } = null!;
    public AppUser Reporter { get; set; } = null!;
    public AppUser ReportedUser { get; set; } = null!;
}