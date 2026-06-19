using System.ComponentModel.DataAnnotations;
using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Swaps;

public class UpdateSwapStatusDto
{
    [Required]
    public SwapStatus Status { get; set; }
}
