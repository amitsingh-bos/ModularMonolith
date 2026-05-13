import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { paymentsApi } from '../api/payments';
import { ordersApi } from '../api/orders';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Select, Textarea, Button } from '../components/FormField';
import { Modal } from '../components/Modal';
import { EmptyState } from '../components/EmptyState';
import { PaymentBadge } from '../components/Badge';
import { PageSpinner } from '../components/Spinner';
import { CreditCard, Plus, ChevronRight } from 'lucide-react';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../api/client';
import type { PaymentDto, PaymentMethod } from '../types';

type ModalType = 'none' | 'create' | 'detail' | 'refund';

const METHODS: PaymentMethod[] = ['CreditCard', 'DebitCard', 'BankTransfer', 'Cash', 'Crypto'];

export default function PaymentsPage() {
  const { tenantId } = useAuth();
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalType>('none');
  const [selected, setSelected] = useState<PaymentDto | null>(null);

  const [form, setForm] = useState({
    orderId: '', amount: 0, currency: 'USD', method: 'CreditCard' as PaymentMethod,
    transactionReference: '', notes: '',
  });
  const [txRef, setTxRef] = useState('');
  const [failReason, setFailReason] = useState('');
  const [refundAmount, setRefundAmount] = useState(0);
  const [refundReason, setRefundReason] = useState('');

  const { data: payments = [], isLoading } = useQuery({
    queryKey: ['payments', tenantId],
    queryFn: () => paymentsApi.getPayments({ tenantId }),
  });

  const { data: orders = [] } = useQuery({
    queryKey: ['orders', tenantId],
    queryFn: () => ordersApi.getOrders({ tenantId }),
  });

  const createMut = useMutation({
    mutationFn: () => paymentsApi.processPayment({
      tenantId, orderId: form.orderId, amount: form.amount, currency: form.currency,
      method: form.method, transactionReference: form.transactionReference || undefined,
      notes: form.notes || undefined,
    }),
    onSuccess: (p) => { qc.invalidateQueries({ queryKey: ['payments'] }); setSelected(p); setModal('detail'); toast.success('Payment initiated'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const completeMut = useMutation({
    mutationFn: () => paymentsApi.completePayment(selected!.id, txRef || undefined),
    onSuccess: (p) => { qc.invalidateQueries({ queryKey: ['payments'] }); setSelected(p); toast.success('Payment completed'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const failMut = useMutation({
    mutationFn: () => paymentsApi.failPayment(selected!.id, failReason || 'Payment declined'),
    onSuccess: (p) => { qc.invalidateQueries({ queryKey: ['payments'] }); setSelected(p); toast.success('Payment marked failed'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const refundMut = useMutation({
    mutationFn: () => paymentsApi.refundPayment(selected!.id, { amount: refundAmount, reason: refundReason || undefined }),
    onSuccess: (p) => { qc.invalidateQueries({ queryKey: ['payments'] }); setSelected(p); setModal('detail'); toast.success('Refund processed'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const openCreate = () => {
    const firstOrder = orders.find(o => o.status !== 'Cancelled');
    setForm({ orderId: firstOrder?.id ?? '', amount: firstOrder?.totalAmount ?? 0, currency: 'USD', method: 'CreditCard', transactionReference: `TXN-${Date.now()}`, notes: '' });
    setModal('create');
  };

  if (isLoading) return <PageSpinner />;

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Payments</h1>
          <p className="text-slate-500 text-sm mt-0.5">{payments.length} payments</p>
        </div>
        <Button onClick={openCreate} size="sm"><Plus size={14} /> Process Payment</Button>
      </div>

      {payments.length === 0 ? (
        <EmptyState icon={CreditCard} title="No payments" description="Process the first payment." action={<Button onClick={openCreate}><Plus size={14} /> Process Payment</Button>} />
      ) : (
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="px-5 py-3 text-left font-medium text-slate-600">Reference</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Method</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Amount</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Status</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Date</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {payments.map(p => (
                <tr key={p.id} className="hover:bg-slate-50 transition-colors cursor-pointer" onClick={() => { setSelected(p); setTxRef(p.transactionReference ?? ''); setModal('detail'); }}>
                  <td className="px-5 py-3 font-mono text-xs text-slate-600">{p.transactionReference ?? '—'}</td>
                  <td className="px-5 py-3 text-slate-700">{p.method}</td>
                  <td className="px-5 py-3 font-semibold text-slate-800">{p.currency} {p.amount.toFixed(2)}</td>
                  <td className="px-5 py-3"><PaymentBadge status={p.status} /></td>
                  <td className="px-5 py-3 text-slate-500">{new Date(p.createdAt).toLocaleDateString()}</td>
                  <td className="px-5 py-3"><ChevronRight size={14} className="text-slate-400 ml-auto" /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create Modal */}
      <Modal title="Process Payment" open={modal === 'create'} onClose={() => setModal('none')}>
        <form onSubmit={e => { e.preventDefault(); createMut.mutate(); }} className="space-y-4">
          <Field label="Order" required>
            <Select value={form.orderId} onChange={e => {
              const o = orders.find(x => x.id === e.target.value);
              setForm(f => ({ ...f, orderId: e.target.value, amount: o?.totalAmount ?? f.amount }));
            }} required>
              <option value="">Select an order</option>
              {orders.filter(o => !['Cancelled', 'Refunded'].includes(o.status)).map(o => (
                <option key={o.id} value={o.id}>{o.orderNumber} — ${o.totalAmount.toFixed(2)} ({o.status})</option>
              ))}
            </Select>
          </Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Amount" required>
              <Input type="number" step="0.01" min="0" value={form.amount} onChange={e => setForm(f => ({ ...f, amount: +e.target.value }))} required />
            </Field>
            <Field label="Currency" required>
              <Select value={form.currency} onChange={e => setForm(f => ({ ...f, currency: e.target.value }))}>
                {['USD', 'EUR', 'GBP', 'INR'].map(c => <option key={c}>{c}</option>)}
              </Select>
            </Field>
          </div>
          <Field label="Payment Method" required>
            <Select value={form.method} onChange={e => setForm(f => ({ ...f, method: e.target.value as PaymentMethod }))}>
              {METHODS.map(m => <option key={m}>{m}</option>)}
            </Select>
          </Field>
          <Field label="Transaction Reference">
            <Input value={form.transactionReference} onChange={e => setForm(f => ({ ...f, transactionReference: e.target.value }))} />
          </Field>
          <Field label="Notes">
            <Textarea value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} rows={2} />
          </Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
            <Button type="submit" loading={createMut.isPending}>Process</Button>
          </div>
        </form>
      </Modal>

      {/* Detail Modal */}
      {selected && (
        <Modal title="Payment Details" open={modal === 'detail'} onClose={() => setModal('none')}>
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <PaymentBadge status={selected.status} />
              <span className="text-sm text-slate-500">{new Date(selected.createdAt).toLocaleString()}</span>
            </div>

            <div className="grid grid-cols-2 gap-3 text-sm">
              {[
                ['Amount', `${selected.currency} ${selected.amount.toFixed(2)}`],
                ['Method', selected.method],
                ['Reference', selected.transactionReference ?? '—'],
                ['Currency', selected.currency],
              ].map(([k, v]) => (
                <div key={k} className="bg-slate-50 rounded-lg p-3">
                  <p className="text-xs text-slate-500">{k}</p>
                  <p className="font-medium text-slate-800 mt-0.5">{v}</p>
                </div>
              ))}
            </div>

            {selected.failureReason && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                <p className="font-medium">Failure Reason</p>
                <p>{selected.failureReason}</p>
              </div>
            )}

            {selected.status === 'Refunded' && (
              <div className="bg-gray-50 border border-gray-200 rounded-lg p-3 text-sm text-slate-700">
                <p className="font-medium">Refunded Amount: {selected.currency} {selected.refundedAmount?.toFixed(2)}</p>
                {selected.refundedAt && <p className="text-slate-500">Refunded at: {new Date(selected.refundedAt).toLocaleString()}</p>}
              </div>
            )}

            {/* Actions for Pending payment */}
            {selected.status === 'Pending' && (
              <div className="space-y-3 pt-1">
                <Field label="Transaction Reference (for completion)">
                  <Input value={txRef} onChange={e => setTxRef(e.target.value)} placeholder="TXN-123456" />
                </Field>
                <div className="flex gap-2">
                  <Button loading={completeMut.isPending} onClick={() => completeMut.mutate()}>Complete Payment</Button>
                  <div className="flex gap-2 flex-1">
                    <Input value={failReason} onChange={e => setFailReason(e.target.value)} placeholder="Failure reason..." className="flex-1" />
                    <Button variant="danger" loading={failMut.isPending} onClick={() => failMut.mutate()}>Mark Failed</Button>
                  </div>
                </div>
              </div>
            )}

            {/* Refund for Completed */}
            {selected.status === 'Completed' && (
              <Button variant="secondary" onClick={() => { setRefundAmount(selected.amount); setRefundReason(''); setModal('refund'); }}>
                Process Refund
              </Button>
            )}
          </div>
        </Modal>
      )}

      {/* Refund Modal */}
      <Modal title="Process Refund" open={modal === 'refund'} onClose={() => setModal('detail')} size="sm">
        <form onSubmit={e => { e.preventDefault(); refundMut.mutate(); }} className="space-y-4">
          <Field label="Refund Amount" required>
            <Input type="number" step="0.01" min="0" max={selected?.amount} value={refundAmount} onChange={e => setRefundAmount(+e.target.value)} required />
          </Field>
          <Field label="Reason">
            <Textarea value={refundReason} onChange={e => setRefundReason(e.target.value)} rows={2} placeholder="Reason for refund..." />
          </Field>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setModal('detail')}>Back</Button>
            <Button type="submit" variant="danger" loading={refundMut.isPending}>Issue Refund</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
