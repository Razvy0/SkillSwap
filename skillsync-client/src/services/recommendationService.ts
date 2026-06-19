import api from './api';
import type { SkillSummary } from './userService';

export interface RecommendationMatch {
  userId: string;
  fullName: string;
  bio?: string;
  rating: number;
  reviewCount: number;
  similarity: number;
  reason: string;
  skills: SkillSummary[];
}

export interface RecommendationsResult {
  generatedAt: string;
  matches: RecommendationMatch[];
}

export const recommendationService = {
  generate: () => api.post<RecommendationsResult>('/recommendations/generate')
};
