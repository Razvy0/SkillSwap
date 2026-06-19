using SkillSwap.Core.DTOs.Recommendations;

namespace SkillSwap.Core.Interfaces.Services;

public interface IRecommendationService
{
    Task<RecommendationsResultDto> GenerateRecommendationsAsync(string userId);
    Task SeedDatabaseAsync();
}
