export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface Category {
  id: number;
  name: string;
  icon: string;
  color: string;
  isSystemCategory: boolean;
  createdAt: string;
}

export interface Expense {
  id: number;
  userId: number;
  categoryId: number;
  category?: Category;
  amount: number;
  description: string;
  date: string;
  receiptUrl?: string;
  createdAt: string;
}

export interface CreateExpenseRequest {
  categoryId: number;
  amount: number;
  description: string;
  date: string;
  receiptUrl?: string;
}

export interface Budget {
  id: number;
  userId: number;
  categoryId: number;
  category?: Category;
  amount: number;
  period: 'Monthly' | 'Weekly' | 'Yearly';
  startDate: string;
  endDate: string;
  createdAt: string;
}

export interface CreateBudgetRequest {
  categoryId: number;
  amount: number;
  period: 'Monthly' | 'Weekly' | 'Yearly';
  startDate: string;
  endDate: string;
}
