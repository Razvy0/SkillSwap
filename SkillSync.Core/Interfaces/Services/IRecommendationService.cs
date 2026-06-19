using SkillSync.Core.DTOs.Recommendations;

namespace SkillSync.Core.Interfaces.Services;

public interface IRecommendationService
{
    Task<RecommendationsResultDto> GenerateRecommendationsAsync(string userId);
    Task SeedDatabaseAsync();
}
