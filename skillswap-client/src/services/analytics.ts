import api from './api';

export interface MonthlyTimeStatsDto {
  month: string;
  numberOfMonth: number;
  year: number;
  earned: number;
  spent: number;
}

export interface CategoryDistributionDto {
  categoryName: string;
  swapCount: number;
}

export interface DashboardAnalyticsDto {
  totalSwapsCompleted: number;
  totalHoursEarned: number;
  totalHoursSpent: number;
  currentRating: number;
  timeStatsOverTime: MonthlyTimeStatsDto[];
  activeCategories: CategoryDistributionDto[];
}

export const getDashboardAnalytics = async () => {
  const { data } = await api.get<DashboardAnalyticsDto>('/Analytics/dashboard');
  return data;
};
