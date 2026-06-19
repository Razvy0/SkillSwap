using SkillSync.Core.Entities;

namespace SkillSync.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByIdWithSkillsAsync(string id);
    Task<IEnumerable<AppUser>> GetByIdsWithSkillsAsync(IEnumerable<string> ids);
    Task UpdateAsync(AppUser user);
    Task<(IEnumerable<AppUser> Items, int TotalCount)> SearchUsersAsync(string? name, string? skill, int page, int pageSize);
    Task<IEnumerable<AppUser>> GetAllAsync();
}
