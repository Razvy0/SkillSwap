using SkillSwap.Core.Enums;

namespace SkillSwap.Core.Entities;

public class Skill
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }
    public bool IsOffering { get; set; }
    public LessonMode LessonMode { get; set; } = LessonMode.Both;
    public int RequiredSessions { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
