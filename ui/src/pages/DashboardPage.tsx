import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '../api/catalog';
import { ordersApi } from '../api/orders';
import { paymentsApi } from '../api/payments';
import { authApi } from '../api/auth';
import { Package, ShoppingCart, CreditCard, Users, AlertCircle } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { OrderBadge, PaymentBadge } from '../components/Badge';

function StatCard({ icon: Icon, label, value, color }: { icon: typeof Package; label: string; value: number | string; color: string }) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 p-5 flex items-center gap-4">
      <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${color}`}>
        <Icon size={22} className="text-white" />
      </div>
      <div>
        <p className="text-sm text-slate-500">{label}</p>
        <p className="text-2xl font-bold text-slate-900">{value}</p>
      </div>
    </div>
  );
}

export default function DashboardPage() {
  const { tenantId } = useAuth();

  const { data: products = [] } = useQuery({
    queryKey: ['products', tenantId],
    queryFn: () => catalogApi.getProducts({ tenantId }),
  });

  const { data: orders = [] } = useQuery({
    queryKey: ['orders', tenantId],
    queryFn: () => ordersApi.getOrders({ tenantId }),
  });

  const { data: payments = [] } = useQuery({
    queryKey: ['payments', tenantId],
    queryFn: () => paymentsApi.getPayments({ tenantId }),
  });

  const { data: users = [] } = useQuery({
    queryKey: ['users', tenantId],
    queryFn: () => authApi.getUsers({ tenantId }),
  });

  //const revenue = payments
  //  .filter(p => p.status === 'Completed')
  //  .reduce((sum, p) => sum + p.amount, 0);
  console.log('payments', payments);
  const revenue = 0;

  //const pendingOrders = orders.filter(o => o.status === 'Pending').length;
  const pendingOrders = 0;
  //const lowStock = products.filter(p => p.stockQuantity < 10).length;
  const lowStock = 0;
  //const recentOrders = [...orders].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 5);
  const recentOrders = [];
  //const recentPayments = [...payments].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 5);
  const recentPayments = [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
        <p className="text-slate-500 text-sm mt-0.5">Overview of your tenant: <span className="font-medium text-slate-700">{tenantId}</span></p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard icon={Package}     label="Products"    value={products.length}  color="bg-emerald-500" />
        <StatCard icon={ShoppingCart} label="Orders"      value={orders.length}    color="bg-orange-500" />
        <StatCard icon={CreditCard}  label="Revenue"     value={`$${revenue.toFixed(2)}`} color="bg-blue-500" />
        <StatCard icon={Users}       label="Users"       value={users.length}     color="bg-rose-500" />
      </div>

      {/* Alerts */}
      {(pendingOrders > 0 || lowStock > 0) && (
        <div className="flex flex-wrap gap-3">
          {pendingOrders > 0 && (
            <div className="flex items-center gap-2 bg-amber-50 border border-amber-200 text-amber-800 rounded-lg px-4 py-2.5 text-sm">
              <AlertCircle size={15} />
              <span><strong>{pendingOrders}</strong> orders awaiting confirmation</span>
            </div>
          )}
          {lowStock > 0 && (
            <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-800 rounded-lg px-4 py-2.5 text-sm">
              <AlertCircle size={15} />
              <span><strong>{lowStock}</strong> products with low stock (&lt;10)</span>
            </div>
          )}
        </div>
      )}

      <div className="grid lg:grid-cols-2 gap-6">
        {/* Recent Orders */}
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <div className="px-5 py-4 border-b border-slate-100 flex items-center gap-2">
            <ShoppingCart size={16} className="text-orange-500" />
            <h2 className="font-semibold text-slate-800">Recent Orders</h2>
          </div>
          {recentOrders.length === 0 ? (
            <p className="text-slate-400 text-sm text-center py-8">No orders yet</p>
          ) : (
            <div className="divide-y divide-slate-100">
              {recentOrders.map(order => (
                <div key={order.id} className="px-5 py-3 flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-slate-800">{order.orderNumber}</p>
                    <p className="text-xs text-slate-500">{new Date(order.createdAt).toLocaleDateString()}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm font-semibold text-slate-700">${order.totalAmount.toFixed(2)}</span>
                    <OrderBadge status={order.status} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Recent Payments */}
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <div className="px-5 py-4 border-b border-slate-100 flex items-center gap-2">
            <CreditCard size={16} className="text-blue-500" />
            <h2 className="font-semibold text-slate-800">Recent Payments</h2>
          </div>
          {recentPayments.length === 0 ? (
            <p className="text-slate-400 text-sm text-center py-8">No payments yet</p>
          ) : (
            <div className="divide-y divide-slate-100">
              {recentPayments.map(payment => (
                <div key={payment.id} className="px-5 py-3 flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-slate-800">{payment.method}</p>
                    <p className="text-xs text-slate-500">{new Date(payment.createdAt).toLocaleDateString()}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm font-semibold text-slate-700">{payment.currency} {payment.amount.toFixed(2)}</span>
                    <PaymentBadge status={payment.status} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
