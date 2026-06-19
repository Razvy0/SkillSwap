using SkillSwap.Core.DTOs.TimeTransactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillSwap.Core.Interfaces.Services
{
    public interface ITimeTransactionService
    {
        Task<IEnumerable<TimeTransactionDto>> GetUserTransactionsAsync(string userId);
    }
}
