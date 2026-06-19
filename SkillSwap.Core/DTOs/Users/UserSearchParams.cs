namespace SkillSwap.Core.DTOs.Users;

public class UserSearchParams
{
    public string? Name { get; set; }
    public string? Skill { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
