using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Swaps;

public class SwapScheduleDto
{
    public int Id { get; set; }
    public SwapTrack Track { get; set; }
    public LessonCadence Cadence { get; set; }
    public int SessionCount { get; set; }
    public string? WeekDays { get; set; }
    public int? TimeOfDayMinutes { get; set; }
    public DateTime? SingleSessionStart { get; set; }
    public DateTime? StartDate { get; set; }
    public string? ChangeRequestNote { get; set; }
    public DateTime? ChangeRequestTime { get; set; }
    public string? ChangeRequestedById { get; set; }
    public DateTime? ChangeRequestedAt { get; set; }
    public SwapScheduleStatus Status { get; set; }
    public string ProposedById { get; set; } = string.Empty;
    public DateTime ProposedAt { get; set; }
}
