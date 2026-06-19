import { useQuery } from '@tanstack/react-query';
import { getDashboardAnalytics } from '@/services/analytics';

export const useDashboardAnalytics = () => {
  return useQuery({
    queryKey: ['analytics', 'dashboard'],
    queryFn: getDashboardAnalytics,
  });
};
