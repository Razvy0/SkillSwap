using SkillSwap.Core.Entities;

namespace SkillSwap.Core.Interfaces.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    Task<double> GetAverageRatingAsync(string userId);
    Task<int> GetReviewCountAsync(string userId);
    Task<bool> HasReviewedSwapAsync(string reviewerId, int swapRequestId);
}
