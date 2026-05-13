import { apiClient } from './client';
import type { OrderDto, CreateOrderRequest, CancelOrderRequest } from '../types';

export const ordersApi = {
  getOrders: (params?: { tenantId?: string; customerId?: string; status?: string }) =>
    apiClient.get<OrderDto[]>('/orders', { params }).then(r => r.data),

  getOrder: (id: string) =>
    apiClient.get<OrderDto>(`/orders/${id}`).then(r => r.data),

  createOrder: (data: CreateOrderRequest) =>
    apiClient.post<OrderDto>('/orders', data).then(r => r.data),

  confirmOrder: (id: string) =>
    apiClient.post<OrderDto>(`/orders/${id}/confirm`).then(r => r.data),

  cancelOrder: (id: string, data: CancelOrderRequest) =>
    apiClient.post<OrderDto>(`/orders/${id}/cancel`, data).then(r => r.data),

  shipOrder: (id: string) =>
    apiClient.post<OrderDto>(`/orders/${id}/ship`).then(r => r.data),

  deliverOrder: (id: string) =>
    apiClient.post<OrderDto>(`/orders/${id}/deliver`).then(r => r.data),
};
