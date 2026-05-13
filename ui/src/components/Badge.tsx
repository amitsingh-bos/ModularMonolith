import type { OrderStatus, PaymentStatus } from '../types';

const orderColors: Record<OrderStatus, string> = {
  Pending:   'bg-yellow-100 text-yellow-800',
  Confirmed: 'bg-blue-100 text-blue-800',
  Shipped:   'bg-indigo-100 text-indigo-800',
  Delivered: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
  Refunded:  'bg-gray-100 text-gray-700',
};

const paymentColors: Record<PaymentStatus, string> = {
  Pending:   'bg-yellow-100 text-yellow-800',
  Completed: 'bg-green-100 text-green-800',
  Failed:    'bg-red-100 text-red-800',
  Refunded:  'bg-gray-100 text-gray-700',
};

export function OrderBadge({ status }: { status: OrderStatus }) {
  return (
    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${orderColors[status]}`}>
      {status}
    </span>
  );
}

export function PaymentBadge({ status }: { status: PaymentStatus }) {
  return (
    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${paymentColors[status]}`}>
      {status}
    </span>
  );
}

export function ActiveBadge({ active }: { active: boolean }) {
  return (
    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${active ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
      {active ? 'Active' : 'Inactive'}
    </span>
  );
}
