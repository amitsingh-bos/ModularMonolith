import axios from 'axios';

const BASE_URL = '/api/v1';

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('tenantId');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data === 'string') return data;
    if (data?.message) return data.message;
    if (data?.title) return data.title;
    if (data?.errors) {
      const errs = Object.values(data.errors).flat() as string[];
      return errs.join(', ');
    }
    return error.message;
  }
  if (error instanceof Error) return error.message;
  return 'An unexpected error occurred';
}
