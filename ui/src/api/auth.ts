import { apiClient } from './client';
import type {
  TokenResponse, LoginRequest, RegisterRequest,
  UserDto, RoleDto, PermissionDto,
  CreateRoleRequest, AssignRoleRequest, AssignPermissionRequest,
} from '../types';

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<TokenResponse>('/auth/login', data).then(r => r.data),

  register: (data: RegisterRequest) =>
    apiClient.post<TokenResponse>('/auth/register', data).then(r => r.data),

  revoke: (refreshToken: string) =>
    apiClient.post('/auth/revoke', { refreshToken }),

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
