using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Disputes;

public class DisputeDto
{
    public int Id { get; set; }
    public int SwapRequestId { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReportedUserId { get; set; } = string.Empty;
    public string ReportedUserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? AdminNotes { get; set; }
}