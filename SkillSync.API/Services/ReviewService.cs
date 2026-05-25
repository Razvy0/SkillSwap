using SkillSync.Core.DTOs.Reviews;
using SkillSync.Core.Entities;
using SkillSync.Core.Enums;
using SkillSync.Core.Exceptions;
using SkillSync.Core.Interfaces.Repositories;
using SkillSync.Core.Interfaces.Services;

namespace SkillSync.API.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly ISwapRepository _swapRepo;
    private readonly IUserRepository _userRepo;

    public ReviewService(IReviewRepository reviewRepo, ISwapRepository swapRepo, IUserRepository userRepo)
    {
        _reviewRepo = reviewRepo;
        _swapRepo = swapRepo;
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
    {
        var reviews = await _reviewRepo.GetReviewsByUserIdAsync(userId);
        return reviews.Select(MapToDto);
    }

    public async Task<ReviewDto> CreateReviewAsync(string reviewerId, CreateReviewDto dto)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(dto.SwapRequestId)
            ?? throw new NotFoundException("SwapRequest", dto.SwapRequestId);

        if (swap.Status != SwapStatus.Completed)
            throw new BadRequestException("You can only review completed swaps.");

        if (swap.RequesterId != reviewerId && swap.ReceiverId != reviewerId)
            throw new UnauthorizedException("You are not a participant in this swap.");

        if (await _reviewRepo.HasReviewedSwapAsync(reviewerId, dto.SwapRequestId))
            throw new BadRequestException("You have already reviewed this swap.");

        var revieweeId = swap.RequesterId == reviewerId ? swap.ReceiverId : swap.RequesterId;

        var review = new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            SwapRequestId = dto.SwapRequestId,
            Score = dto.Score,
            Comment = dto.Comment
        };

        await _reviewRepo.AddAsync(review);

        // Update user rating
        var newRating = await _reviewRepo.GetAverageRatingAsync(revieweeId);
        var reviewee = await _userRepo.GetByIdAsync(revieweeId);
        if (reviewee != null)
        {
            reviewee.Rating = newRating;
            await _userRepo.UpdateAsync(reviewee);
        }

        review.Reviewer = (await _userRepo.GetByIdAsync(reviewerId))!;
        review.Reviewee = reviewee!;
        return MapToDto(review);
    }

    private static ReviewDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        ReviewerId = r.ReviewerId,
        ReviewerName = r.Reviewer?.FullName ?? "",
        RevieweeId = r.RevieweeId,
        RevieweeName = r.Reviewee?.FullName ?? "",
        SwapRequestId = r.SwapRequestId,
        Score = r.Score,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };

    public async Task<bool> HasReviewedSwapAsync(string reviewerId, int swapRequestId)
        => await _reviewRepo.HasReviewedSwapAsync(reviewerId, swapRequestId);

    public async Task<IEnumerable<ReviewableSwapDto>> GetReviewableSwapsAsync(string currentUserId, string otherUserId)
    {
        var swaps = await _swapRepo.GetSwapsByUserIdAsync(currentUserId);
        var completedWithOther = swaps.Where(s =>
            s.Status == SwapStatus.Completed &&
            ((s.RequesterId == currentUserId && s.ReceiverId == otherUserId) ||
             (s.ReceiverId == currentUserId && s.RequesterId == otherUserId)));

        var result = new List<ReviewableSwapDto>();
        foreach (var swap in completedWithOther)
        {
            if (!await _reviewRepo.HasReviewedSwapAsync(currentUserId, swap.Id))
            {
                result.Add(new ReviewableSwapDto
                {
                    SwapId = swap.Id,
                    LessonType = swap.LessonType,
                    TeacherId = swap.TeacherId,
                    LearnerId = swap.LearnerId,
                    OfferedSkillTitle = swap.OfferedSkill?.Title ?? "",
                    RequestedSkillTitle = swap.RequestedSkill?.Title ?? "",
                    CompletedAt = swap.CreatedAt
                });
            }
        }
        return result;
    }
}
