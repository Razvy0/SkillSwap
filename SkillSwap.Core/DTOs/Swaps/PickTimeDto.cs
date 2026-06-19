using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Core.DTOs.Swaps;

public class PickTimeDto
{
    [Required]
    public DateTime ScheduledDate { get; set; }
}
