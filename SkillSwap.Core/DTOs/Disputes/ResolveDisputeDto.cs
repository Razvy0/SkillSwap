namespace SkillSwap.Core.DTOs.Disputes;
public class ResolveDisputeDto
{
    public DisputeAction Action { get; set; }
    public string AdminNotes { get; set; } = string.Empty;
}

public enum DisputeAction
{
    BanUser,
    DeleteSwap,
    Dismiss
}