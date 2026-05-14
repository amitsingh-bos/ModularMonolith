import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { AuthUser, LoginRequest, RegisterRequest } from '../types';
import { authApi } from '../api/auth';
import { getErrorMessage } from '../api/client';

export interface LoginResult {
  requiresTwoFactor?: boolean;
  twoFactorToken?: string;
  twoFactorMethod?: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  tenantId: string;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<LoginResult>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  verifyTwoFactor: (twoFactorToken: string, code: string, tenantId: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function parseJwt(token: string): Record<string, unknown> {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return {};
  }
}

function claimsToUser(token: string): AuthUser | null {
  const claims = parseJwt(token);
  if (!claims.sub) return null;

  const permissions = Array.isArray(claims['permission'])
    ? (claims['permission'] as string[])
    : claims['permission']
    ? [claims['permission'] as string]
    : [];

  const roles = Array.isArray(claims['role'])
    ? (claims['role'] as string[])
    : claims['role']
    ? [claims['role'] as string]
    : [];

  return {
    id: String(claims.sub),
    email: String(claims.email ?? ''),
    tenantId: String(claims.tenant_id ?? localStorage.getItem('tenantId') ?? ''),
    permissions,
    roles,
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const token = localStorage.getItem('accessToken');
    return token ? claimsToUser(token) : null;
  });

  const [tenantId, setTenantId] = useState<string>(
    () => localStorage.getItem('tenantId') ?? ''
  );

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    if (token) setUser(claimsToUser(token));
  }, []);

  const login = useCallback(async (data: LoginRequest): Promise<LoginResult> => {
    try {
      const resp = await authApi.login(data);
      const tokenData = resp.data;

      if (tokenData.requiresTwoFactor) {
        return {
          requiresTwoFactor: true,
          twoFactorToken: tokenData.twoFactorToken,
          twoFactorMethod: tokenData.twoFactorMethod,
        };
      }

      localStorage.setItem('accessToken', tokenData.accessToken);
      localStorage.setItem('refreshToken', tokenData.refreshToken);
      localStorage.setItem('tenantId', data.tenantId);
      setTenantId(data.tenantId);
      setUser(claimsToUser(tokenData.accessToken));
      return {};
    } catch (err) {
      throw new Error(getErrorMessage(err));
    }
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    try {
      const resp = await authApi.register(data);
      localStorage.setItem('accessToken', resp.data.accessToken);
      localStorage.setItem('refreshToken', resp.data.refreshToken);
      localStorage.setItem('tenantId', data.tenantId);
      setTenantId(data.tenantId);
      setUser(claimsToUser(resp.data.accessToken));
    } catch (err) {
      throw new Error(getErrorMessage(err));
    }
  }, []);

  const verifyTwoFactor = useCallback(async (twoFactorToken: string, code: string, tid: string) => {
    try {
      const resp = await authApi.verifyLogin2fa({ twoFactorToken, code });
      const tokenData = resp.data;
      localStorage.setItem('accessToken', tokenData.accessToken);
      localStorage.setItem('refreshToken', tokenData.refreshToken);
      localStorage.setItem('tenantId', tid);
      setTenantId(tid);
      setUser(claimsToUser(tokenData.accessToken));
    } catch (err) {
      throw new Error(getErrorMessage(err));
    }
  }, []);

  const logout = useCallback(() => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) authApi.revoke(refreshToken).catch(() => {});
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tenantId');
    setUser(null);
    setTenantId('');
  }, []);

  return (
    <AuthContext.Provider value={{ user, tenantId, isAuthenticated: !!user, login, register, logout, verifyTwoFactor }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
