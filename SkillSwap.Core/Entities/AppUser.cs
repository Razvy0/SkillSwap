using Microsoft.AspNetCore.Identity;

namespace SkillSwap.Core.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int TimeBalance { get; set; } = 5;
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public ICollection<TimeTransaction> TimeTransactions { get; set; } = new List<TimeTransaction>();
    public ICollection<Dispute> DisputesReported { get; set; } = new List<Dispute>();
    public ICollection<Dispute> DisputesReceived { get; set; } = new List<Dispute>();
}
