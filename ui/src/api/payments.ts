import { apiClient } from './client';
import type { PaymentDto, ProcessPaymentRequest, RefundPaymentRequest } from '../types';

export const paymentsApi = {
  getPayments: (params?: { tenantId?: string; orderId?: string; status?: string }) =>
    apiClient.get<PaymentDto[]>('/payments', { params }).then(r => r.data),

  getPayment: (id: string) =>
    apiClient.get<PaymentDto>(`/payments/${id}`).then(r => r.data),

  getPaymentByOrder: (orderId: string) =>
    apiClient.get<PaymentDto>(`/payments/order/${orderId}`).then(r => r.data),

  processPayment: (data: ProcessPaymentRequest) =>
    apiClient.post<PaymentDto>('/payments', data).then(r => r.data),

  completePayment: (id: string, transactionReference?: string) =>
    apiClient.post<PaymentDto>(`/payments/${id}/complete`, null, {
      params: { transactionReference },
    }).then(r => r.data),

  failPayment: (id: string, failureReason: string) =>
    apiClient.post<PaymentDto>(`/payments/${id}/fail`, null, {
      params: { failureReason },
    }).then(r => r.data),

  refundPayment: (id: string, data: RefundPaymentRequest) =>
    apiClient.post<PaymentDto>(`/payments/${id}/refund`, data).then(r => r.data),
};
