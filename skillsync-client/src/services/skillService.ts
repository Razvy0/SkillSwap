import api from './api';
import { PagedResult } from './userService';

export interface Skill {
  id: number;
  title: string;
  description?: string;
  proficiencyLevel: string;
  isOffering: boolean;
  lessonMode: LessonMode;
  requiredSessions: number;
  categoryName: string;
  categoryId: number;
  userId: string;
  userFullName: string;
  createdAt: string;
}

export type LessonMode = 'SingleOnly' | 'RecurringOnly' | 'Both';

export interface CreateSkillDto {
  title: string;
  description?: string;
  categoryId: number;
  proficiencyLevel: number;
  isOffering: boolean;
  lessonMode: LessonMode;
  requiredSessions: number;
}

export interface SkillQueryParams {
  category?: string;
  search?: string;
  isOffering?: boolean;
  page?: number;
  pageSize?: number;
}

export const skillService = {
  getSkills: (params?: SkillQueryParams) => api.get<PagedResult<Skill>>('/skills', { params }),
  getSkill: (id: number) => api.get<Skill>(`/skills/${id}`),
  getMySkills: () => api.get<Skill[]>('/skills/my'),
  getUserSkills: (userId: string) => api.get<Skill[]>(`/skills/user/${userId}`),
  createSkill: (dto: CreateSkillDto) => api.post<Skill>('/skills', dto),
  deleteSkill: (id: number) => api.delete(`/skills/${id}`),
};
