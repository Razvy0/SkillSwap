import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { reviewService, CreateReviewDto } from '@/services/reviewService';

export function useUserReviews(userId: string) {
    return useQuery({
        queryKey: ['reviews', userId],
        queryFn: () => reviewService.getByUser(userId).then((r) => r.data),
        enabled: !!userId,
    });
}

export function useHasReviewed(swapRequestId: number) {
    return useQuery({
        queryKey: ['reviews', 'check', swapRequestId],
        queryFn: () => reviewService.hasReviewed(swapRequestId).then((r) => r.data.hasReviewed),
        enabled: swapRequestId > 0,
    });
}

export function useReviewableSwaps(otherUserId: string) {
    return useQuery({
        queryKey: ['reviews', 'reviewable', otherUserId],
        queryFn: () => reviewService.getReviewableSwaps(otherUserId).then((r) => r.data),
        enabled: !!otherUserId,
    });
}

export function useCreateReview() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (dto: CreateReviewDto) => reviewService.create(dto),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['reviews'] });
            qc.invalidateQueries({ queryKey: ['user'] });
            qc.invalidateQueries({ queryKey: ['swaps'] });
        },
    });
}
