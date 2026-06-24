using SkillSwap.Core.DTOs.Disputes;

namespace SkillSwap.Core.Interfaces.Services;

public interface IDisputeService
{
    Task<DisputeDto> CreateDisputeAsync(string currentUserId, CreateDisputeDto dto);
    Task<IEnumerable<DisputeDto>> GetUserDisputesAsync(string userId);
    Task ResolveDisputeAsync(int disputeId, ResolveDisputeDto dto, string adminId);
    Task<IEnumerable<DisputeDto>> GetAllDisputesAsync();
}