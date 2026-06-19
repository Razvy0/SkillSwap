import api from './api';

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  fullName: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  expiration: string;
}

export const authService = {
  login: (dto: LoginDto) => api.post<AuthResponse>('/auth/login', dto),
  register: (dto: RegisterDto) => api.post<AuthResponse>('/auth/register', dto),
};
