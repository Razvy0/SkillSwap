using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Core.DTOs.Disputes;

public class CreateDisputeDto
{
    [Required]
    public int SwapRequestId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 2000 characters.")]
    public string Reason { get; set; } = string.Empty;
}