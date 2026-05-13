import { apiClient } from './client';
import type {
  CategoryDto, CreateCategoryRequest, UpdateCategoryRequest,
  ProductDto, CreateProductRequest, UpdateProductRequest, AdjustStockRequest,
} from '../types';

export const catalogApi = {
  // Categories
  getCategories: () =>
    apiClient.get<CategoryDto[]>('/categories').then(r => r.data),

  getCategory: (id: string) =>
    apiClient.get<CategoryDto>(`/categories/${id}`).then(r => r.data),

  createCategory: (data: CreateCategoryRequest) =>
    apiClient.post<CategoryDto>('/categories', data).then(r => r.data),

  updateCategory: (id: string, data: UpdateCategoryRequest) =>
    apiClient.put<CategoryDto>(`/categories/${id}`, data).then(r => r.data),

  deleteCategory: (id: string) =>
    apiClient.delete(`/categories/${id}`),

  // Products
  getProducts: (params?: { tenantId?: string; categoryId?: string; search?: string }) =>
    apiClient.get<ProductDto[]>('/products', { params }).then(r => r.data),

  getProduct: (id: string) =>
    apiClient.get<ProductDto>(`/products/${id}`).then(r => r.data),

  createProduct: (data: CreateProductRequest) =>
    apiClient.post<ProductDto>('/products', data).then(r => r.data),

  updateProduct: (id: string, data: UpdateProductRequest) =>
    apiClient.put<ProductDto>(`/products/${id}`, data).then(r => r.data),

  deleteProduct: (id: string) =>
    apiClient.delete(`/products/${id}`),

  adjustStock: (id: string, data: AdjustStockRequest) =>
    apiClient.patch(`/products/${id}/stock`, data),
};
