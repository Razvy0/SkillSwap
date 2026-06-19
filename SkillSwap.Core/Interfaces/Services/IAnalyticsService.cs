using SkillSwap.Core.DTOs.Analytics;

namespace SkillSwap.Core.Interfaces.Services;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(string userId);
}
