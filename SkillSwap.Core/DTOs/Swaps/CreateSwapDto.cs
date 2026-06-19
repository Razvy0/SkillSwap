using System.ComponentModel.DataAnnotations;
using SkillSwap.Core.Enums;

namespace SkillSwap.Core.DTOs.Swaps;

public class CreateSwapDto
{
    public int? OfferedSkillId { get; set; }

    [Required]
    public int RequestedSkillId { get; set; }

    public LessonType LessonType { get; set; } = LessonType.OneWay;
    public LessonCadence RequestedCadence { get; set; } = LessonCadence.Single;
    public LessonCadence? OfferedCadence { get; set; }
    public TwoWayScheduleMode TwoWayScheduleMode { get; set; } = TwoWayScheduleMode.Separate;

    public DateTime? ScheduledDate { get; set; }
}
