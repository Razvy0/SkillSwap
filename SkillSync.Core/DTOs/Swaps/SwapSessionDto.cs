using SkillSync.Core.Enums;

namespace SkillSync.Core.DTOs.Swaps;

public class SwapSessionDto
{
    public int Id { get; set; }
    public SwapTrack Track { get; set; }
    public int SessionOrder { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SwapSessionStatus Status { get; set; }
    public bool RequesterValidated { get; set; }
    public bool ReceiverValidated { get; set; }
}
