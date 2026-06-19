namespace SkillSwap.Core.DTOs.Users;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int TimeBalance { get; set; }
    public double Rating { get; set; }
    public List<SkillSummaryDto> Skills { get; set; } = new();
}

public class SkillSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsOffering { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
