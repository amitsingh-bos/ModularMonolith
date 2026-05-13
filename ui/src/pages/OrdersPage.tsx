import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ordersApi } from '../api/orders';
import { catalogApi } from '../api/catalog';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Textarea, Button } from '../components/FormField';
import { Modal } from '../components/Modal';
import { EmptyState } from '../components/EmptyState';
import { OrderBadge } from '../components/Badge';
import { PageSpinner } from '../components/Spinner';
import { ShoppingCart, Plus, ChevronRight, X } from 'lucide-react';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../api/client';
import type { OrderDto, CreateOrderItemRequest, OrderStatus } from '../types';

type ModalType = 'none' | 'create' | 'detail' | 'cancel';

const transitions: Record<OrderStatus, OrderStatus | null> = {
  Pending: 'Confirmed',
  Confirmed: 'Shipped',
  Shipped: 'Delivered',
  Delivered: null,
  Cancelled: null,
  Refunded: null,
};

const actionLabels: Partial<Record<OrderStatus, string>> = {
  Confirmed: 'Confirm Order',
  Shipped: 'Mark Shipped',
  Delivered: 'Mark Delivered',
};

export default function OrdersPage() {
  const { tenantId } = useAuth();
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalType>('none');
  const [selected, setSelected] = useState<OrderDto | null>(null);

  const [form, setForm] = useState({
    customerId: '', addressLine1: '', addressLine2: '', city: 'New York', country: 'US', postalCode: '10001', notes: '',
  });
  const [items, setItems] = useState<CreateOrderItemRequest[]>([]);
  const [cancelReason, setCancelReason] = useState('');

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ['orders', tenantId],
    queryFn: () => ordersApi.getOrders({ tenantId }),
  });

  const { data: products = [] } = useQuery({
    queryKey: ['products', tenantId],
    queryFn: () => catalogApi.getProducts({ tenantId }),
  });

  const createMut = useMutation({
    mutationFn: () => ordersApi.createOrder({
      tenantId,
      customerId: form.customerId,
      shippingAddressLine1: form.addressLine1,
      shippingAddressLine2: form.addressLine2 || undefined,
      shippingCity: form.city,
      shippingCountry: form.country,
      shippingPostalCode: form.postalCode,
      notes: form.notes || undefined,
      items,
    }),
    onSuccess: (order) => { qc.invalidateQueries({ queryKey: ['orders'] }); setModal('detail'); setSelected(order); toast.success('Order created'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const advanceMut = useMutation({
    mutationFn: async (order: OrderDto) => {
      if (order.status === 'Pending') return ordersApi.confirmOrder(order.id);
      if (order.status === 'Confirmed') return ordersApi.shipOrder(order.id);
      if (order.status === 'Shipped') return ordersApi.deliverOrder(order.id);
    },
    onSuccess: (updated) => {
      qc.invalidateQueries({ queryKey: ['orders'] });
      if (updated) setSelected(updated);
      toast.success('Order updated');
    },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const cancelMut = useMutation({
    mutationFn: () => ordersApi.cancelOrder(selected!.id, { reason: cancelReason || undefined }),
    onSuccess: (updated) => { qc.invalidateQueries({ queryKey: ['orders'] }); setSelected(updated); setModal('detail'); toast.success('Order cancelled'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const openCreate = () => {
    setForm({ customerId: crypto.randomUUID(), addressLine1: '123 Main St', addressLine2: '', city: 'New York', country: 'US', postalCode: '10001', notes: '' });
    setItems([]);
    setModal('create');
  };

  const addItem = () => {
    const p = products[0];
    if (!p) return;
    setItems(prev => [...prev, { productId: p.id, quantity: 1, unitPrice: p.price }]);
  };

  const removeItem = (i: number) => setItems(prev => prev.filter((_, idx) => idx !== i));
  const updateItem = (i: number, field: keyof CreateOrderItemRequest, value: string | number) =>
    setItems(prev => prev.map((item, idx) => idx === i ? { ...item, [field]: value } : item));

  const total = items.reduce((sum, i) => sum + i.quantity * i.unitPrice, 0);

  if (isLoading) return <PageSpinner />;

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Orders</h1>
          <p className="text-slate-500 text-sm mt-0.5">{orders.length} orders</p>
        </div>
        <Button onClick={openCreate} size="sm"><Plus size={14} /> New Order</Button>
      </div>

      {orders.length === 0 ? (
        <EmptyState icon={ShoppingCart} title="No orders" description="Create the first order." action={<Button onClick={openCreate}><Plus size={14} /> New Order</Button>} />
      ) : (
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="px-5 py-3 text-left font-medium text-slate-600">Order #</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Items</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Total</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Status</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Date</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {orders.map(order => (
                <tr key={order.id} className="hover:bg-slate-50 transition-colors cursor-pointer" onClick={() => { setSelected(order); setModal('detail'); }}>
                  <td className="px-5 py-3 font-mono font-medium text-slate-800 text-xs">{order.orderNumber}</td>
                  <td className="px-5 py-3 text-slate-600">{order.items.length} item{order.items.length !== 1 ? 's' : ''}</td>
                  <td className="px-5 py-3 font-semibold text-slate-800">${order.totalAmount.toFixed(2)}</td>
                  <td className="px-5 py-3"><OrderBadge status={order.status} /></td>
                  <td className="px-5 py-3 text-slate-500">{new Date(order.createdAt).toLocaleDateString()}</td>
                  <td className="px-5 py-3"><ChevronRight size={14} className="text-slate-400 ml-auto" /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create Modal */}
      <Modal title="New Order" open={modal === 'create'} onClose={() => setModal('none')} size="lg">
        <form onSubmit={e => { e.preventDefault(); if (items.length === 0) { toast.error('Add at least one item'); return; } createMut.mutate(); }} className="space-y-5">
          <div className="grid grid-cols-2 gap-3">
            <Field label="Address Line 1" required><Input value={form.addressLine1} onChange={e => setForm(f => ({ ...f, addressLine1: e.target.value }))} required /></Field>
            <Field label="Address Line 2"><Input value={form.addressLine2} onChange={e => setForm(f => ({ ...f, addressLine2: e.target.value }))} /></Field>
            <Field label="City" required><Input value={form.city} onChange={e => setForm(f => ({ ...f, city: e.target.value }))} required /></Field>
            <Field label="Country" required><Input value={form.country} onChange={e => setForm(f => ({ ...f, country: e.target.value }))} required /></Field>
            <Field label="Postal Code" required><Input value={form.postalCode} onChange={e => setForm(f => ({ ...f, postalCode: e.target.value }))} required /></Field>
          </div>
          <Field label="Notes"><Textarea value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} rows={2} /></Field>

          {/* Items */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <p className="text-sm font-medium text-slate-700">Order Items</p>
              <Button type="button" variant="ghost" size="sm" onClick={addItem}><Plus size={13} /> Add Item</Button>
            </div>
            {items.length === 0 ? (
              <div className="border-2 border-dashed border-slate-200 rounded-lg p-6 text-center text-slate-400 text-sm">
                No items added. Click "Add Item" to start.
              </div>
            ) : (
              <div className="space-y-2">
                {items.map((item, i) => {
                  return (
                    <div key={i} className="flex items-center gap-2 bg-slate-50 rounded-lg p-2">
                      <select
                        value={item.productId}
                        onChange={e => {
                          const prod = products.find(x => x.id === e.target.value);
                          if (prod) updateItem(i, 'productId', prod.id);
                          if (prod) updateItem(i, 'unitPrice', prod.price);
                        }}
                        className="flex-1 text-sm px-2 py-1.5 rounded border border-slate-300 bg-white"
                      >
                        {products.map(p => <option key={p.id} value={p.id}>{p.name} (${p.price})</option>)}
                      </select>
                      <input type="number" min="1" value={item.quantity} onChange={e => updateItem(i, 'quantity', +e.target.value)}
                        className="w-16 text-sm px-2 py-1.5 rounded border border-slate-300 text-center" />
                      <span className="text-sm text-slate-600 w-16 text-right">${(item.quantity * item.unitPrice).toFixed(2)}</span>
                      <button type="button" onClick={() => removeItem(i)} className="p-1 rounded hover:bg-red-50 text-slate-400 hover:text-red-500"><X size={13} /></button>
                    </div>
                  );
                })}
                <div className="flex justify-end pt-1">
                  <span className="text-sm font-semibold text-slate-800">Total: ${total.toFixed(2)}</span>
                </div>
              </div>
            )}
          </div>

          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
            <Button type="submit" loading={createMut.isPending}>Create Order</Button>
          </div>
        </form>
      </Modal>

      {/* Detail Modal */}
      {selected && (
        <Modal title={`Order ${selected.orderNumber}`} open={modal === 'detail'} onClose={() => setModal('none')} size="lg">
          <div className="space-y-5">
            <div className="flex items-center gap-3">
              <OrderBadge status={selected.status} />
              <span className="text-sm text-slate-500">{new Date(selected.createdAt).toLocaleString()}</span>
            </div>

            {/* Shipping */}
            <div className="bg-slate-50 rounded-lg p-4 text-sm text-slate-700">
              <p className="font-medium text-slate-800 mb-1">Shipping Address</p>
              <p>{selected.shippingAddressLine1}</p>
              {selected.shippingAddressLine2 && <p>{selected.shippingAddressLine2}</p>}
              <p>{selected.shippingCity}, {selected.shippingCountry} {selected.shippingPostalCode}</p>
            </div>

            {/* Items */}
            <div>
              <p className="text-sm font-medium text-slate-800 mb-2">Items</p>
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-200">
                    <th className="pb-2 text-left text-xs font-medium text-slate-500">Product</th>
                    <th className="pb-2 text-right text-xs font-medium text-slate-500">Qty</th>
                    <th className="pb-2 text-right text-xs font-medium text-slate-500">Unit</th>
                    <th className="pb-2 text-right text-xs font-medium text-slate-500">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {selected.items.map(item => (
                    <tr key={item.id}>
                      <td className="py-2">
                        <p className="font-medium text-slate-800">{item.productName}</p>
                        <p className="text-xs text-slate-500">{item.productSku}</p>
                      </td>
                      <td className="py-2 text-right text-slate-600">{item.quantity}</td>
                      <td className="py-2 text-right text-slate-600">${item.unitPrice.toFixed(2)}</td>
                      <td className="py-2 text-right font-medium text-slate-800">${item.totalPrice.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="border-t border-slate-200">
                    <td colSpan={3} className="pt-2 text-right font-semibold text-slate-700">Total</td>
                    <td className="pt-2 text-right font-bold text-slate-900">${selected.totalAmount.toFixed(2)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>

            {/* Actions */}
            {(() => {
              const next = transitions[selected.status];
              const canCancel = selected.status === 'Pending' || selected.status === 'Confirmed';
              return (next || canCancel) ? (
                <div className="flex gap-2 pt-1">
                  {next && (
                    <Button loading={advanceMut.isPending} onClick={() => advanceMut.mutate(selected)}>
                      {actionLabels[next]}
                    </Button>
                  )}
                  {canCancel && (
                    <Button variant="danger" onClick={() => { setCancelReason(''); setModal('cancel'); }}>Cancel Order</Button>
                  )}
                </div>
              ) : null;
            })()}
          </div>
        </Modal>
      )}

      {/* Cancel Modal */}
      <Modal title="Cancel Order" open={modal === 'cancel'} onClose={() => setModal('detail')} size="sm">
        <form onSubmit={e => { e.preventDefault(); cancelMut.mutate(); }} className="space-y-4">
          <Field label="Reason (optional)">
            <Textarea value={cancelReason} onChange={e => setCancelReason(e.target.value)} rows={3} placeholder="Why is this order being cancelled?" />
          </Field>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setModal('detail')}>Back</Button>
            <Button type="submit" variant="danger" loading={cancelMut.isPending}>Confirm Cancel</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
