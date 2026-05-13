// ─── Auth ─────────────────────────────────────────────────────────────────────
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  statusCode: number;
  errors: string[];
  pagination?: unknown;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginRequest {
  tenantId: string;
  email: string;
  password: string;
  deviceInfo?: string;
}

export interface RegisterRequest {
  tenantId: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UserDto {
  id: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  isEmailVerified: boolean;
  lastLoginAt?: string;
  roles: string[];
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
}

export interface PermissionDto {
  id: string;
  code: string;
  description: string;
  module: string;
}

export interface CreateRoleRequest {
  tenantId: string;
  name: string;
  description?: string;
}

export interface AssignRoleRequest {
  userId: string;
  roleId: string;
}

export interface AssignPermissionRequest {
  permissionId: string;
}

// ─── Catalog ──────────────────────────────────────────────────────────────────
export interface CategoryDto {
  id: string;
  tenantId: string;
  name: string;
  slug: string;
  description?: string;
  parentCategoryId?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCategoryRequest {
  tenantId: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
}

export interface ProductDto {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryName: string;
  name: string;
  description?: string;
  sku: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProductRequest {
  tenantId: string;
  categoryId: string;
  name: string;
  description?: string;
  sku: string;
  price: number;
  stockQuantity: number;
}

export interface UpdateProductRequest {
  name: string;
  description?: string;
  price: number;
  categoryId: string;
}

export interface AdjustStockRequest {
  quantity: number;
}

// ─── Orders ───────────────────────────────────────────────────────────────────
export interface OrderItemDto {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderDto {
  id: string;
  tenantId: string;
  customerId: string;
  orderNumber: string;
  status: OrderStatus;
  shippingAddressLine1: string;
  shippingAddressLine2?: string;
  shippingCity: string;
  shippingCountry: string;
  shippingPostalCode: string;
  notes?: string;
  totalAmount: number;
  items: OrderItemDto[];
  createdAt: string;
  updatedAt?: string;
}

export type OrderStatus = 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled' | 'Refunded';

export interface CreateOrderItemRequest {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderRequest {
  tenantId: string;
  customerId: string;
  shippingAddressLine1: string;
  shippingAddressLine2?: string;
  shippingCity: string;
  shippingCountry: string;
  shippingPostalCode: string;
  notes?: string;
  items: CreateOrderItemRequest[];
}

export interface CancelOrderRequest {
  reason?: string;
}

// ─── Payments ─────────────────────────────────────────────────────────────────
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded';
export type PaymentMethod = 'CreditCard' | 'DebitCard' | 'BankTransfer' | 'Cash' | 'Crypto';

export interface PaymentDto {
  id: string;
  tenantId: string;
  orderId: string;
  amount: number;
  currency: string;
  method: PaymentMethod;
  status: PaymentStatus;
  transactionReference?: string;
  failureReason?: string;
  notes?: string;
  processedAt?: string;
  refundedAt?: string;
  refundedAmount?: number;
  createdAt: string;
  updatedAt?: string;
}

export interface ProcessPaymentRequest {
  tenantId: string;
  orderId: string;
  amount: number;
  currency: string;
  method: PaymentMethod;
  transactionReference?: string;
  gatewayResponse?: string;
  notes?: string;
}

export interface RefundPaymentRequest {
  amount: number;
  reason?: string;
}

// ─── API wrapper ──────────────────────────────────────────────────────────────
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface AuthUser {
  id: string;
  email: string;
  tenantId: string;
  permissions: string[];
  roles: string[];
}
