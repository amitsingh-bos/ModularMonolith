import { apiClient } from './client';
import type { ApiResponse, OrderDto, CreateOrderRequest, CancelOrderRequest } from '../types';

export const ordersApi = {
  getOrders: (params?: { tenantId?: string; customerId?: string; status?: string }) =>
    apiClient.get<ApiResponse<OrderDto[]>>('/orders', { params }).then(r => r.data.data),

  getOrder: (id: string) =>
    apiClient.get<ApiResponse<OrderDto>>(`/orders/${id}`).then(r => r.data.data),

  createOrder: (data: CreateOrderRequest) =>
    apiClient.post<ApiResponse<OrderDto>>('/orders', data).then(r => r.data.data),

  confirmOrder: (id: string) =>
    apiClient.post<ApiResponse<OrderDto>>(`/orders/${id}/confirm`).then(r => r.data.data),

  cancelOrder: (id: string, data: CancelOrderRequest) =>
    apiClient.post<OrderDto>(`/orders/${id}/cancel`, data).then(r => r.data),

  shipOrder: (id: string) =>
    apiClient.post<OrderDto>(`/orders/${id}/ship`).then(r => r.data),

  deliverOrder: (id: string) =>
    apiClient.post<OrderDto>(`/orders/${id}/deliver`).then(r => r.data),
};
