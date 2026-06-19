import { useMutation } from '@tanstack/react-query';
import { recommendationService } from '@/services/recommendationService';

export function useGenerateRecommendations() {
  return useMutation({
    mutationFn: () => recommendationService.generate().then((r) => r.data)
  });
}
