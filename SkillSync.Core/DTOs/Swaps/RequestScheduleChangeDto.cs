using System.ComponentModel.DataAnnotations;
using SkillSync.Core.Enums;

namespace SkillSync.Core.DTOs.Swaps;

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
