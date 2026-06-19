using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<AppUser?> GetByIdAsync(string id)
        => await _context.Users.FindAsync(id);

    public async Task<AppUser?> GetByIdWithSkillsAsync(string id)
        => await _context.Users
            .Include(u => u.Skills).ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<AppUser>> GetByIdsWithSkillsAsync(IEnumerable<string> ids)
        => await _context.Users
            .Include(u => u.Skills).ThenInclude(s => s.Category)
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();

    public async Task UpdateAsync(AppUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<AppUser> Items, int TotalCount)> SearchUsersAsync(string? name, string? skill, int page, int pageSize)
    {
        var query = _context.Users
            .Include(u => u.Skills).ThenInclude(s => s.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(skill))
        {
            query = query.Where(u =>
                u.FullName.ToLower().Contains(name.ToLower())
                || u.Skills.Any(s =>
                    s.Title.ToLower().Contains(skill.ToLower())
                    || s.Category.Name.ToLower().Contains(skill.ToLower())));
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(u => u.FullName.ToLower().Contains(name.ToLower()));
        }
        else if (!string.IsNullOrWhiteSpace(skill))
        {
            query = query.Where(u => u.Skills.Any(s =>
                s.Title.ToLower().Contains(skill.ToLower())
                || s.Category.Name.ToLower().Contains(skill.ToLower())));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }
}
