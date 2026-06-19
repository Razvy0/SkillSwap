using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.DTOs.Reviews;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;

namespace SkillSwap.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
    {
        var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
        return Ok(reviews);
    }

    [HttpGet("check/{swapRequestId}")]
    public async Task<IActionResult> HasReviewed(int swapRequestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var hasReviewed = await _reviewService.HasReviewedSwapAsync(userId, swapRequestId);
        return Ok(new { hasReviewed });
    }

    [HttpGet("reviewable/{otherUserId}")]
    public async Task<IActionResult> GetReviewableSwaps(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var swaps = await _reviewService.GetReviewableSwapsAsync(userId, otherUserId);
        return Ok(swaps);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var review = await _reviewService.CreateReviewAsync(userId, dto);
        return CreatedAtAction(nameof(GetByUser), new { userId = review.RevieweeId }, review);
    }
}
