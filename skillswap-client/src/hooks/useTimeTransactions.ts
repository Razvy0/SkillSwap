import { useQuery } from '@tanstack/react-query';
import { getMyTimeTransactions } from '@/services/timeTransactions';

export const useMyTimeTransactions = () => {
  return useQuery({
    queryKey: ['timeTransactions', 'my'],
    queryFn: getMyTimeTransactions,
  });
};
