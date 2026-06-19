import api from './api';

export interface UserProfile {
    id: string;
    email: string;
    fullName: string;
    bio?: string;
    timeBalance: number;
    rating: number;
    skills: SkillSummary[];
}

export interface SkillSummary {
    id: number;
    title: string;
    isOffering: boolean;
    categoryName: string;
}

export interface UserSearchResult {
    id: string;
    fullName: string;
    bio?: string;
    rating: number;
    skills: SkillSummary[];
}

export interface UpdateProfileDto {
    fullName?: string;
    bio?: string;
}

export interface UserSearchParams {
    name?: string;
    skill?: string;
    page?: number;
    pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export const userService = {
    getMe: () =>
        api.get<UserProfile>('/users/me'),
    getProfile: (id: string) =>
        api.get<UserProfile>(`/users/${id}`),
    updateProfile: (dto: UpdateProfileDto) =>
        api.put<UserProfile>('/users/profile', dto),
    searchUsers: (params?: UserSearchParams) =>
        api.get<PagedResult<UserSearchResult>>('/users/search', { params }),
};
