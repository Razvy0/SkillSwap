using System.ComponentModel.DataAnnotations;
using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Swaps;

public class RequestScheduleChangeDto
{
    [Required]
    public SwapTrack Track { get; set; }

    [Required]
    [MaxLength(100)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public DateTime SuggestedTime { get; set; }
}
