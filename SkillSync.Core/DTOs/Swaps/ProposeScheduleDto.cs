using System.ComponentModel.DataAnnotations;

namespace SkillSync.Core.DTOs.Swaps;

public class ProposeScheduleDto
{
    [Required]
    public ICollection<ScheduleTrackProposalDto> Tracks { get; set; } = new List<ScheduleTrackProposalDto>();
}
