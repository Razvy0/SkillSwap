import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import disputeService, { CreateDisputeDto } from '@/services/disputeService';

export const useDisputes = () => {
  return useQuery({
    queryKey: ['disputes'],
    queryFn: () => disputeService.getMyDisputes(),
  });
};

export const useCreateDispute = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dto: CreateDisputeDto) => disputeService.createDispute(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['swaps'] });
      queryClient.invalidateQueries({ queryKey: ['disputes'] });
    },
  });
};