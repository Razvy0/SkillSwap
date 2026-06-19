using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
        => await _dbSet
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Where(r => r.RevieweeId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<double> GetAverageRatingAsync(string userId)
    {
        var reviews = await _dbSet.Where(r => r.RevieweeId == userId).ToListAsync();
        return reviews.Count > 0 ? reviews.Average(r => r.Score) : 0;
    }

    public async Task<int> GetReviewCountAsync(string userId)
        => await _dbSet.CountAsync(r => r.RevieweeId == userId);

    public async Task<bool> HasReviewedSwapAsync(string reviewerId, int swapRequestId)
        => await _dbSet.AnyAsync(r => r.ReviewerId == reviewerId && r.SwapRequestId == swapRequestId);
}
