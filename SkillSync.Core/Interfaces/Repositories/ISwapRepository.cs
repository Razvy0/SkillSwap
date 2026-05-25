using SkillSync.Core.Entities;

namespace SkillSync.Core.Interfaces.Repositories;

public interface ISwapRepository : IRepository<SwapRequest>
{
    Task<IEnumerable<SwapRequest>> GetSwapsByUserIdAsync(string userId);
    Task<SwapRequest?> GetSwapWithDetailsAsync(int id);
    Task<bool> HasActiveSwapForSkillAsync(int skillId);
}
