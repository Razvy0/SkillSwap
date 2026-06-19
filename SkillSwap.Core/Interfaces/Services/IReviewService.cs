using SkillSwap.Core.DTOs.Reviews;

namespace SkillSwap.Core.Interfaces.Services;

public interface IReviewService
{
    Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
    Task<ReviewDto> CreateReviewAsync(string reviewerId, CreateReviewDto dto);
    Task<bool> HasReviewedSwapAsync(string reviewerId, int swapRequestId);
    Task<IEnumerable<ReviewableSwapDto>> GetReviewableSwapsAsync(string currentUserId, string otherUserId);
}
