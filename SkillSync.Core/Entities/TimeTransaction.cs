using SkillSync.Core.Enums;
using System;

namespace SkillSync.Core.Entities
{
    public class TimeTransaction
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
        public int Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public int? SwapRequestId { get; set; }
        public SwapRequest? SwapRequest { get; set; }
        public int? SwapSessionId { get; set; }
        public SwapSession? SwapSession { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
