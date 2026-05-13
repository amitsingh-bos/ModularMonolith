import { apiClient } from './client';
import type {
  ApiResponse,CategoryDto, CreateCategoryRequest, UpdateCategoryRequest,
  ProductDto, CreateProductRequest, UpdateProductRequest, AdjustStockRequest,
} from '../types';

export const catalogApi = {
  // Categories
  getCategories: () =>
    apiClient.get<ApiResponse<CategoryDto[]>>('/categories').then(r => r.data.data),

  getCategory: (id: string) =>
    apiClient.get<ApiResponse<CategoryDto>>(`/categories/${id}`).then(r => r.data.data),

  createCategory: (data: CreateCategoryRequest) =>
    apiClient.post<ApiResponse<CategoryDto>>('/categories', data).then(r => r.data.data),

  updateCategory: (id: string, data: UpdateCategoryRequest) =>
    apiClient.put<ApiResponse<CategoryDto>>(`/categories/${id}`, data).then(r => r.data.data),

  deleteCategory: (id: string) =>
    apiClient.delete(`/categories/${id}`),

  // Products
  getProducts: (params?: { tenantId?: string; categoryId?: string; search?: string }) =>
    apiClient.get<ApiResponse<ProductDto[]>>('/products', { params }).then(r => r.data.data),

  getProduct: (id: string) =>
    apiClient.get<ApiResponse<ProductDto>>(`/products/${id}`).then(r => r.data.data),

  createProduct: (data: CreateProductRequest) =>
    apiClient.post<ApiResponse<ProductDto>>('/products', data).then(r => r.data.data),

  updateProduct: (id: string, data: UpdateProductRequest) =>
    apiClient.put<ApiResponse<ProductDto>>(`/products/${id}`, data).then(r => r.data.data),

  deleteProduct: (id: string) =>
    apiClient.delete(`/products/${id}`),

  adjustStock: (id: string, data: AdjustStockRequest) =>
    apiClient.patch(`/products/${id}/stock`, data),
};
