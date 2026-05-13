import { apiClient } from './client';
import type {ApiResponse, PaymentDto, ProcessPaymentRequest, RefundPaymentRequest } from '../types';

export const paymentsApi = {
  getPayments: (params?: { tenantId?: string; orderId?: string; status?: string }) =>
    apiClient.get<ApiResponse<PaymentDto[]>>('/payments', { params }).then(r => r.data.data),

  getPayment: (id: string) =>
    apiClient.get<ApiResponse<PaymentDto>>(`/payments/${id}`).then(r => r.data.data),

  getPaymentByOrder: (orderId: string) =>
    apiClient.get<ApiResponse<PaymentDto>>(`/payments/order/${orderId}`).then(r => r.data.data),

  processPayment: (data: ProcessPaymentRequest) =>
    apiClient.post<ApiResponse<PaymentDto>>('/payments', data).then(r => r.data.data),

  completePayment: (id: string, transactionReference?: string) =>
    apiClient.post<ApiResponse<PaymentDto>>(`/payments/${id}/complete`, null, {
      params: { transactionReference },
    }).then(r => r.data),

  failPayment: (id: string, failureReason: string) =>
    apiClient.post<PaymentDto>(`/payments/${id}/fail`, null, {
      params: { failureReason },
    }).then(r => r.data),

  refundPayment: (id: string, data: RefundPaymentRequest) =>
    apiClient.post<PaymentDto>(`/payments/${id}/refund`, data).then(r => r.data),
};
