using System.ComponentModel.DataAnnotations;
using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Skills;

public class CreateSkillDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Beginner;
    public bool IsOffering { get; set; } = true;
    public LessonMode LessonMode { get; set; } = LessonMode.Both;
    [Range(1, 8)]
    public int RequiredSessions { get; set; } = 1;
}
