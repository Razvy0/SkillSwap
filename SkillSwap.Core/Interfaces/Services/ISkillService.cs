using SkillSwap.Core.DTOs.Common;
using SkillSwap.Core.DTOs.Skills;

namespace SkillSwap.Core.Interfaces.Services;

public interface ISkillService
{
    Task<PagedResult<SkillDto>> GetSkillsAsync(SkillQueryParams queryParams);
    Task<SkillDto> GetSkillByIdAsync(int id);
    Task<SkillDto> CreateSkillAsync(string userId, CreateSkillDto dto);
    Task DeleteSkillAsync(string userId, int id);
    Task<IEnumerable<SkillDto>> GetUserSkillsAsync(string userId);
}
