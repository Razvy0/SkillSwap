using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Core.Interfaces.Services;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace SkillSwap.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IServiceScopeFactory _scopeFactory;

    public RecommendationsController(
        IRecommendationService recommendationService,
        IServiceScopeFactory scopeFactory) // <-- Add this
    {
        _recommendationService = recommendationService;
        _scopeFactory = scopeFactory; // <-- Add this
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var results = await _recommendationService.GenerateRecommendationsAsync(userId);
        return Ok(results);
    }
    [HttpPost("seed-database")]
    [AllowAnonymous]
    public IActionResult SeedDatabase()
    {
        // Start the background thread
        _ = Task.Run(async () =>
        {
            // 1. Create a brand new, independent lifespan
            using var scope = _scopeFactory.CreateScope();
            
            // 2. Safely grab a fresh RecommendationService out of this new lifespan
            var scopedRecommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
            
            // 3. Run the hour-long process safely!
            await scopedRecommendationService.SeedDatabaseAsync();
        });

        return Accepted(new { message = "Database seeding has started in the background. Check your C# terminal for progress!" });
    }
    
}
