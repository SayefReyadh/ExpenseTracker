import api from './api';
import { Expense, CreateExpenseRequest } from '../types';

export const expenseService = {
  async getExpenses(): Promise<Expense[]> {
    const response = await api.get<Expense[]>('/expenses');
    return response.data;
  },

  async getExpense(id: number): Promise<Expense> {
    const response = await api.get<Expense>(`/expenses/${id}`);
    return response.data;
  },

  async createExpense(data: CreateExpenseRequest): Promise<Expense> {
    const response = await api.post<Expense>('/expenses', data);
    return response.data;
  },

  async updateExpense(id: number, data: CreateExpenseRequest): Promise<Expense> {
    const response = await api.put<Expense>(`/expenses/${id}`, data);
    return response.data;
  },

  async deleteExpense(id: number): Promise<void> {
    await api.delete(`/expenses/${id}`);
  },
};
