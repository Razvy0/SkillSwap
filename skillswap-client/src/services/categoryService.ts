import api from './api';

export interface Category {
  id: number;
  name: string;
  description?: string;
}

export const categoryService = {
  getAll: () => api.get<Category[]>('/categories'),
};
