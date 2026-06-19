using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.DTOs.Analytics;
using SkillSwap.Core.Enums;
using SkillSwap.Core.Interfaces.Services;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.API.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(string userId)
    {
        var result = new DashboardAnalyticsDto();

        // Basic Stats
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            result.CurrentRating = user.Rating;
        }

        var userSwapsQuery = _context.SwapRequests
            .Include(s => s.OfferedSkill).ThenInclude(s => s.Category)
            .Include(s => s.RequestedSkill).ThenInclude(s => s.Category)
            .Where(s => (s.RequesterId == userId || s.ReceiverId == userId) && s.Status == SwapStatus.Completed);

        result.TotalSwapsCompleted = await userSwapsQuery.CountAsync();

        // Transactions
        var userTransactions = _context.TimeTransactions
            .Where(t => t.UserId == userId && (t.TransactionType == TransactionType.Earned || t.TransactionType == TransactionType.Spent));

        // Grouping locally for time over time (EF Core sometimes struggles with complex DateTime grouping in Postgres/SQL)
        var limitDate = DateTime.UtcNow.AddMonths(-6);
        var recentTransactions = await userTransactions
            .Where(t => t.CreatedAt >= limitDate)
            .ToListAsync();

        result.TotalHoursEarned = await userTransactions.Where(t => t.TransactionType == TransactionType.Earned).SumAsync(t => t.Amount);
        // Sometimes amount is saved differently depending on logic, earlier we saw Spent was 0 and EscrowHold was -1. 
        // Let's actually calculate "Spent" based on completed swaps or EscrowHold. 
        // Wait, from previous message: Spent is 0, EscrowHold is -1. 
        // So Escrow holds are the actual spend when it completes. 
        // Let's adjust to count EscrowHold as spent if the swap is completed.
        // Actually, let's just count the completed swaps where user is requester * 1hr to represent Spent times.
        var totalSpentSwaps = await _context.SwapRequests.CountAsync(s => s.RequesterId == userId && s.Status == SwapStatus.Completed);
        result.TotalHoursSpent = totalSpentSwaps;

        // Group by month
        var monthlyStats = new Dictionary<string, MonthlyTimeStatsDto>();
        for (int i = 5; i >= 0; i--)
        {
            var d = DateTime.UtcNow.AddMonths(-i);
            var key = $"{d.Year}-{d.Month}";
            monthlyStats[key] = new MonthlyTimeStatsDto
            {
                Month = d.ToString("MMM"),
                NumberOfMonth = d.Month,
                Year = d.Year,
                Earned = 0,
                Spent = 0
            };
        }

        foreach (var t in recentTransactions)
        {
            if (t.TransactionType == TransactionType.Earned)
            {
                var key = $"{t.CreatedAt.Year}-{t.CreatedAt.Month}";
                if (monthlyStats.ContainsKey(key))
                {
                    monthlyStats[key].Earned += t.Amount;
                }
            }
        }

        // To get monthly spent, let's look at completed swaps where user is requester
        var recentCompletedSwaps = await _context.SwapRequests
            .Where(s => s.RequesterId == userId && s.Status == SwapStatus.Completed && s.UpdatedAt >= limitDate)
            .ToListAsync();

        foreach (var s in recentCompletedSwaps)
        {
            var dt = s.UpdatedAt ?? s.CreatedAt;
            var key = $"{dt.Year}-{dt.Month}";
            if (monthlyStats.ContainsKey(key))
            {
                monthlyStats[key].Spent += 1; // Assuming each swap is 1hr
            }
        }

        result.TimeStatsOverTime = monthlyStats.Values.OrderBy(m => m.Year).ThenBy(m => m.NumberOfMonth).ToList();

        // Categories Map
        var categoryMap = new Dictionary<string, int>();
        var allSwaps = await userSwapsQuery.ToListAsync();
        
        foreach (var swap in allSwaps)
        {
            // If I am requester, I received the RequestedSkill's category training
            // If I am receiver, I received the OfferedSkill's category training? No, receiver gives the RequestedSkill, Requester gives OfferedSkill.
            // Let's just group all involved categories.
            var cat1 = swap.OfferedSkill?.Category?.Name;
            var cat2 = swap.RequestedSkill?.Category?.Name;

            if (cat1 != null) {
                if (!categoryMap.ContainsKey(cat1)) categoryMap[cat1] = 0;
                categoryMap[cat1]++;
            }
            if (cat2 != null && cat2 != cat1) {
                if (!categoryMap.ContainsKey(cat2)) categoryMap[cat2] = 0;
                categoryMap[cat2]++;
            }
        }

        result.ActiveCategories = categoryMap.Select(kvp => new CategoryDistributionDto
        {
            CategoryName = kvp.Key,
            SwapCount = kvp.Value
        })
        .OrderByDescending(c => c.SwapCount)
        .Take(5)
        .ToList();

        return result;
    }
}
