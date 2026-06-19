using SkillSwap.Core.Enums;

namespace SkillSwap.Core.Entities;

public class SwapRequest
{
    public int Id { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public LessonType LessonType { get; set; } = LessonType.OneWay;
    public LessonCadence RequestedCadence { get; set; } = LessonCadence.Single;
    public LessonCadence? OfferedCadence { get; set; }
    public TwoWayScheduleMode TwoWayScheduleMode { get; set; } = TwoWayScheduleMode.Separate;
    public string? TeacherId { get; set; }
    public string? LearnerId { get; set; }
    public int? OfferedSkillId { get; set; }
    public int RequestedSkillId { get; set; }
    public SwapStatus Status { get; set; } = SwapStatus.Pending;
    public DateTime? ScheduledDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Time-slot scheduling: receiver proposes a window, requester picks a time
    public DateTime? TimeSlotStart { get; set; }
    public DateTime? TimeSlotEnd { get; set; }

    // Validation: both parties must confirm the swap was fulfilled
    public bool RequesterValidated { get; set; }
    public bool ReceiverValidated { get; set; }

    public AppUser Requester { get; set; } = null!;
    public AppUser Receiver { get; set; } = null!;
    public Skill? OfferedSkill { get; set; }
    public Skill RequestedSkill { get; set; } = null!;
    public ICollection<SwapSchedule> Schedules { get; set; } = new List<SwapSchedule>();
    public ICollection<SwapSession> Sessions { get; set; } = new List<SwapSession>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
}
