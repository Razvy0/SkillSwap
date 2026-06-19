using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Skills;

public class SkillDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }
    public bool IsOffering { get; set; }
    public LessonMode LessonMode { get; set; }
    public int RequiredSessions { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
