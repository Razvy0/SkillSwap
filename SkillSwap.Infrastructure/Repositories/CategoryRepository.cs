using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<Category?> GetByNameAsync(string name)
        => await _dbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
}
