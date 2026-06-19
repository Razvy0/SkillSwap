namespace SkillSwap.Core.DTOs.Skills;

public class SkillQueryParams
{
    public string? Category { get; set; }
    public string? Search { get; set; }
    public bool? IsOffering { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
