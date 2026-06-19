using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Enums;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.Infrastructure.Repositories;

public class SwapRepository : Repository<SwapRequest>, ISwapRepository
{
    public SwapRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SwapRequest>> GetSwapsByUserIdAsync(string userId)
        => await _dbSet
            .Include(s => s.Requester)
            .Include(s => s.Receiver)
            .Include(s => s.OfferedSkill)
            .Include(s => s.RequestedSkill)
            .Include(s => s.Schedules)
            .Include(s => s.Sessions)
            .Where(s => s.RequesterId == userId || s.ReceiverId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<SwapRequest?> GetSwapWithDetailsAsync(int id)
        => await _dbSet
            .Include(s => s.Requester)
            .Include(s => s.Receiver)
            .Include(s => s.OfferedSkill)
            .Include(s => s.RequestedSkill)
            .Include(s => s.Schedules)
            .Include(s => s.Sessions)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> HasActiveSwapForSkillAsync(int skillId)
    {
        var activeStatuses = new[]
        {
            SwapStatus.Pending,
            SwapStatus.Accepted,
            SwapStatus.Scheduled,
            SwapStatus.ValidatedByRequester,
            SwapStatus.ValidatedByReceiver,
            SwapStatus.Disputed
        };

        return await _dbSet.AnyAsync(s =>
            (s.RequestedSkillId == skillId || s.OfferedSkillId == skillId)
            && activeStatuses.Contains(s.Status));
    }
}
