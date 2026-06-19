using SkillSwap.Core.Entities;

namespace SkillSwap.Core.Interfaces.Repositories;

public interface ITimeTransactionRepository
{
    Task<TimeTransaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeTransaction>> GetByUserIdAsync(string userId);
    Task AddAsync(TimeTransaction transaction);
    Task UpdateAsync(TimeTransaction transaction);
}
