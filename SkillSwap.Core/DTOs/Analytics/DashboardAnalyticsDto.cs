using System;
using System.Collections.Generic;

namespace SkillSwap.Core.DTOs.Analytics
{
    public class DashboardAnalyticsDto
    {
        public int TotalSwapsCompleted { get; set; }
        public int TotalHoursEarned { get; set; }
        public int TotalHoursSpent { get; set; }
        public double CurrentRating { get; set; }
        
        public List<MonthlyTimeStatsDto> TimeStatsOverTime { get; set; } = new();
        public List<CategoryDistributionDto> ActiveCategories { get; set; } = new();
    }

    public class MonthlyTimeStatsDto
    {
        public string Month { get; set; } = string.Empty;
        public int NumberOfMonth { get; set; }
        public int Year { get; set; }
        public int Earned { get; set; }
        public int Spent { get; set; }
    }

    public class CategoryDistributionDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int SwapCount { get; set; }
    }
}