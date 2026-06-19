using SkillSwap.Core.Enums;
using System;

namespace SkillSwap.Core.DTOs.TimeTransactions
{
    public class TimeTransactionDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public int? SwapRequestId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
