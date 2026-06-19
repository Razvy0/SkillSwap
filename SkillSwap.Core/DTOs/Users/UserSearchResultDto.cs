namespace SkillSwap.Core.DTOs.Users;

public class UserSearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public double Rating { get; set; }
    public List<SkillSummaryDto> Skills { get; set; } = new();
}
