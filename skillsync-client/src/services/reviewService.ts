import api from './api';
import type { LessonType } from './swapService';

export interface ReviewDto {
    id: number;
    reviewerId: string;
    reviewerName: string;
    revieweeId: string;
    revieweeName: string;
    swapRequestId: number;
    score: number;
    comment?: string;
    createdAt: string;
}

export interface CreateReviewDto {
    swapRequestId: number;
    score: number;
    comment?: string;
}

export interface ReviewableSwap {
    swapId: number;
    lessonType: LessonType;
    teacherId?: string | null;
    learnerId?: string | null;
    offeredSkillTitle: string;
    requestedSkillTitle: string;
    completedAt: string;
}

export const reviewService = {
    getByUser: (userId: string) =>
        api.get<ReviewDto[]>(`/reviews/user/${userId}`),
    create: (dto: CreateReviewDto) =>
        api.post<ReviewDto>('/reviews', dto),
    hasReviewed: (swapRequestId: number) =>
        api.get<{ hasReviewed: boolean }>(`/reviews/check/${swapRequestId}`),
    getReviewableSwaps: (otherUserId: string) =>
        api.get<ReviewableSwap[]>(`/reviews/reviewable/${otherUserId}`),
};
