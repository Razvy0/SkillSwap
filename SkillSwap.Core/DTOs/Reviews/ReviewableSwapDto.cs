using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Reviews;

public class ReviewableSwapDto
{
    public int SwapId { get; set; }
    public LessonType LessonType { get; set; }
    public string? TeacherId { get; set; }
    public string? LearnerId { get; set; }
    public string OfferedSkillTitle { get; set; } = string.Empty;
    public string RequestedSkillTitle { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}
