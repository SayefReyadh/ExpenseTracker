import api from './api';
import { Category } from '../types';

export const categoryService = {
  async getCategories(): Promise<Category[]> {
    const response = await api.get<Category[]>('/categories');
    return response.data;
  },

  async createCategory(data: { name: string; icon: string; color: string }): Promise<Category> {
    const response = await api.post<Category>('/categories', data);
    return response.data;
  },
};
