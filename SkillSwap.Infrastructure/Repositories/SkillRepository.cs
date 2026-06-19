using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.Infrastructure.Repositories;

public class SkillRepository : Repository<Skill>, ISkillRepository
{
    public SkillRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Skill> Items, int TotalCount)> GetSkillsWithDetailsAsync(
        string? category, string? search, bool? isOffering, int page, int pageSize)
    {
        var query = _dbSet
            .Include(s => s.User)
            .Include(s => s.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(s => s.Category.Name.ToLower() == category.ToLower());

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Title.ToLower().Contains(search.ToLower())
                || (s.Description != null && s.Description.ToLower().Contains(search.ToLower())));

        if (isOffering.HasValue)
            query = query.Where(s => s.IsOffering == isOffering.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Skill>> GetSkillsByUserIdAsync(string userId)
        => await _dbSet.Include(s => s.Category)
            .Where(s => s.UserId == userId)
            .ToListAsync();

    public async Task<Skill?> GetSkillWithDetailsAsync(int id)
        => await _dbSet.Include(s => s.User).Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id);
}
