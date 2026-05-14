import { apiClient } from './client';
import type {
  ApiResponse,
  TokenResponse, LoginRequest, RegisterRequest,
  UserDto, RoleDto, PermissionDto,
  CreateRoleRequest, AssignRoleRequest, AssignPermissionRequest,
  Setup2FaResponse, TwoFactorStatus,
} from '../types';

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<ApiResponse<TokenResponse>>('/auth/login', data).then(r => r.data),

  register: (data: RegisterRequest) =>
    apiClient.post<ApiResponse<TokenResponse>>('/auth/register', data).then(r => r.data),

  revoke: (refreshToken: string) =>
    apiClient.post('/auth/revoke', { refreshToken }),

  // 2FA
  get2faStatus: () =>
    apiClient.get<ApiResponse<TwoFactorStatus>>('/auth/2fa/status').then(r => r.data),

  setup2fa: (data: { method: string; phoneNumber?: string }) =>
    apiClient.post<ApiResponse<Setup2FaResponse>>('/auth/2fa/setup', data).then(r => r.data),

  confirm2faSetup: (data: { code: string }) =>
    apiClient.post<ApiResponse<null>>('/auth/2fa/confirm', data).then(r => r.data),

  disable2fa: (data: { code: string }) =>
    apiClient.delete<ApiResponse<null>>('/auth/2fa', { data }).then(r => r.data),

  verifyLogin2fa: (data: { twoFactorToken: string; code: string }) =>
    apiClient.post<ApiResponse<TokenResponse>>('/auth/2fa/verify', data).then(r => r.data),

  // Users
  getUsers: (params?: { tenantId?: string; page?: number; pageSize?: number }) =>
    apiClient.get<UserDto[]>('/users', { params }).then(r => r.data),

  getUser: (id: string) =>
    apiClient.get<UserDto>(`/users/${id}`).then(r => r.data),

  assignRole: (userId: string, data: AssignRoleRequest) =>
    apiClient.post(`/users/${userId}/roles`, data),

  // Roles
  getRoles: () =>
    apiClient.get<RoleDto[]>('/roles').then(r => r.data),

  getRole: (id: string) =>
    apiClient.get<RoleDto>(`/roles/${id}`).then(r => r.data),

  createRole: (data: CreateRoleRequest) =>
    apiClient.post<RoleDto>('/roles', data).then(r => r.data),

  deleteRole: (id: string) =>
    apiClient.delete(`/roles/${id}`),

  assignPermission: (roleId: string, data: AssignPermissionRequest) =>
    apiClient.post(`/roles/${roleId}/permissions`, data),

  removePermission: (roleId: string, permissionId: string) =>
    apiClient.delete(`/roles/${roleId}/permissions/${permissionId}`),

  // Permissions
  getPermissions: () =>
    apiClient.get<PermissionDto[]>('/permissions').then(r => r.data),
};
