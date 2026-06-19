using System.ComponentModel.DataAnnotations;
using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Swaps;

public class ScheduleTrackProposalDto
{
    [Required]
    public SwapTrack Track { get; set; }

    [Required]
    public LessonCadence Cadence { get; set; }

    [Required]
    public int SessionCount { get; set; }

    public DateTime? SingleSessionStart { get; set; }

    public DateTime? StartDate { get; set; }

    public ICollection<DayOfWeek>? WeekDays { get; set; }

    public int? TimeOfDayMinutes { get; set; }
}
