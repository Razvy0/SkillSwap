using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Core.DTOs.Reviews;

public class CreateReviewDto
{
    [Required]
    public int SwapRequestId { get; set; }

    [Required, Range(1, 5)]
    public int Score { get; set; }

    public string? Comment { get; set; }
}
