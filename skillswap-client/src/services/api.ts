import axios from 'axios';
import { useAuthStore } from '@/stores/authStore';

const api = axios.create({
  baseURL: 'https://localhost:5001/api',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const requestUrl: string | undefined = error.config?.url;
    const isAuthRequest = requestUrl?.includes('/auth/login') || requestUrl?.includes('/auth/register');

    if (status === 401 && !isAuthRequest) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
