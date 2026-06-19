using SkillSwap.Core.Entities;

namespace SkillSwap.Core.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}
