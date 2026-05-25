using SkillSync.Core.Enums;

namespace SkillSync.Core.Entities;

public class SwapSession
{
    public int Id { get; set; }
    public int SwapRequestId { get; set; }
    public int? SwapScheduleId { get; set; }
    public SwapTrack Track { get; set; }
    public int SessionOrder { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SwapSessionStatus Status { get; set; } = SwapSessionStatus.Proposed;
    public bool RequesterValidated { get; set; }
    public bool ReceiverValidated { get; set; }
    public DateTime? ValidatedAt { get; set; }

    public SwapRequest SwapRequest { get; set; } = null!;
    public SwapSchedule? SwapSchedule { get; set; }
}
