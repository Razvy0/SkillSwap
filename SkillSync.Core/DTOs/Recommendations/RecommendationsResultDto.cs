using SkillSync.Core.DTOs.Users;

namespace SkillSync.Core.DTOs.Recommendations;

public class RecommendationsResultDto
{
    public DateTime GeneratedAt { get; set; }
    public List<RecommendationMatchDto> Matches { get; set; } = new();
}

public class RecommendationMatchDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public double Similarity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<SkillSummaryDto> Skills { get; set; } = new();
}
