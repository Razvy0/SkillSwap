using SkillSync.Core.Enums;

namespace SkillSync.Core.DTOs.Swaps;

public class SwapDto
{
    public int Id { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public LessonType LessonType { get; set; }
    public LessonCadence RequestedCadence { get; set; }
    public LessonCadence? OfferedCadence { get; set; }
    public TwoWayScheduleMode TwoWayScheduleMode { get; set; }
    public string? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public string? LearnerId { get; set; }
    public string? LearnerName { get; set; }
    public string OfferedSkillTitle { get; set; } = string.Empty;
    public string RequestedSkillTitle { get; set; } = string.Empty;
    public int RequestedSessionCount { get; set; }
    public int? OfferedSessionCount { get; set; }
    public SwapStatus Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? TimeSlotStart { get; set; }
    public DateTime? TimeSlotEnd { get; set; }
    public bool RequesterValidated { get; set; }
    public bool ReceiverValidated { get; set; }
    public ICollection<SwapScheduleDto> Schedules { get; set; } = new List<SwapScheduleDto>();
    public ICollection<SwapSessionDto> Sessions { get; set; } = new List<SwapSessionDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
