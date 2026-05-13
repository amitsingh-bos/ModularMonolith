import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { catalogApi } from '../api/catalog';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Select, Textarea, Button } from '../components/FormField';
import { Modal } from '../components/Modal';
import { EmptyState } from '../components/EmptyState';
import { ActiveBadge } from '../components/Badge';
import { PageSpinner } from '../components/Spinner';
import { Package, Plus, Pencil, Trash2, BarChart2, Search } from 'lucide-react';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../api/client';
import type { ProductDto } from '../types';

type ModalType = 'none' | 'create' | 'edit' | 'delete' | 'stock';

export default function ProductsPage() {
  const { tenantId } = useAuth();
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalType>('none');
  const [target, setTarget] = useState<ProductDto | null>(null);
  const [search, setSearch] = useState('');

  const [form, setForm] = useState({
    categoryId: '', name: '', description: '', sku: '', price: 0, stockQuantity: 0,
  });
  const [stockQty, setStockQty] = useState(0);

  const { data: products = [], isLoading } = useQuery({
    queryKey: ['products', tenantId],
    queryFn: () => catalogApi.getProducts({ tenantId }),
  });

  const { data: categories = [] } = useQuery({
    queryKey: ['categories'],
    queryFn: catalogApi.getCategories,
  });

  const createMut = useMutation({
    mutationFn: () => catalogApi.createProduct({ tenantId, ...form, description: form.description || undefined }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['products'] }); setModal('none'); toast.success('Product created'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const updateMut = useMutation({
    mutationFn: () => catalogApi.updateProduct(target!.id, { name: form.name, description: form.description || undefined, price: form.price, categoryId: form.categoryId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['products'] }); setModal('none'); toast.success('Product updated'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => catalogApi.deleteProduct(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['products'] }); setModal('none'); toast.success('Product deleted'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const stockMut = useMutation({
    mutationFn: () => catalogApi.adjustStock(target!.id, { quantity: stockQty }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['products'] }); setModal('none'); toast.success('Stock updated'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const openCreate = () => {
    setForm({ categoryId: categories[0]?.id ?? '', name: '', description: '', sku: '', price: 0, stockQuantity: 0 });
    setModal('create');
  };
  const openEdit = (p: ProductDto) => { setTarget(p); setForm({ categoryId: p.categoryId, name: p.name, description: p.description ?? '', sku: p.sku, price: p.price, stockQuantity: p.stockQuantity }); setModal('edit'); };
  const openStock = (p: ProductDto) => { setTarget(p); setStockQty(0); setModal('stock'); };

  const filtered = products.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase()) ||
    p.sku.toLowerCase().includes(search.toLowerCase())
  );

  if (isLoading) return <PageSpinner />;

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Products</h1>
          <p className="text-slate-500 text-sm mt-0.5">{products.length} products</p>
        </div>
        <Button onClick={openCreate} size="sm"><Plus size={14} /> Add Product</Button>
      </div>

      {/* Search */}
      <div className="relative max-w-sm">
        <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search by name or SKU..."
          className="w-full pl-9 pr-3 py-2 rounded-lg border border-slate-300 text-sm focus:outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
        />
      </div>

      {filtered.length === 0 ? (
        <EmptyState icon={Package} title="No products" description="Add your first product to get started." action={<Button onClick={openCreate}><Plus size={14} /> Add Product</Button>} />
      ) : (
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="px-5 py-3 text-left font-medium text-slate-600">Name</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">SKU</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Category</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Price</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Stock</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Status</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filtered.map(p => (
                <tr key={p.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-5 py-3">
                    <div>
                      <p className="font-medium text-slate-800">{p.name}</p>
                      {p.description && <p className="text-xs text-slate-500 truncate max-w-xs">{p.description}</p>}
                    </div>
                  </td>
                  <td className="px-5 py-3 font-mono text-xs text-slate-500">{p.sku}</td>
                  <td className="px-5 py-3 text-slate-600">{p.categoryName}</td>
                  <td className="px-5 py-3 font-semibold text-slate-800">${p.price.toFixed(2)}</td>
                  <td className="px-5 py-3">
                    <span className={`font-medium ${p.stockQuantity < 10 ? 'text-red-600' : 'text-slate-700'}`}>{p.stockQuantity}</span>
                  </td>
                  <td className="px-5 py-3"><ActiveBadge active={p.isActive} /></td>
                  <td className="px-5 py-3">
                    <div className="flex items-center gap-1 justify-end">
                      <button onClick={() => openStock(p)} className="p-1.5 rounded hover:bg-blue-50 text-slate-400 hover:text-blue-600" title="Adjust Stock"><BarChart2 size={14} /></button>
                      <button onClick={() => openEdit(p)} className="p-1.5 rounded hover:bg-slate-100 text-slate-500 hover:text-slate-700"><Pencil size={14} /></button>
                      <button onClick={() => { setTarget(p); setModal('delete'); }} className="p-1.5 rounded hover:bg-red-50 text-slate-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create Modal */}
      <Modal title="Add Product" open={modal === 'create'} onClose={() => setModal('none')}>
        <form onSubmit={e => { e.preventDefault(); createMut.mutate(); }} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Field label="Name" required><Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required /></Field>
            <Field label="SKU" required><Input value={form.sku} onChange={e => setForm(f => ({ ...f, sku: e.target.value }))} required /></Field>
          </div>
          <Field label="Category" required>
            <Select value={form.categoryId} onChange={e => setForm(f => ({ ...f, categoryId: e.target.value }))} required>
              <option value="">Select category</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </Select>
          </Field>
          <Field label="Description"><Textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={2} /></Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Price" required><Input type="number" step="0.01" min="0" value={form.price} onChange={e => setForm(f => ({ ...f, price: +e.target.value }))} required /></Field>
            <Field label="Stock Qty" required><Input type="number" min="0" value={form.stockQuantity} onChange={e => setForm(f => ({ ...f, stockQuantity: +e.target.value }))} required /></Field>
          </div>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
            <Button type="submit" loading={createMut.isPending}>Create</Button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal title="Edit Product" open={modal === 'edit'} onClose={() => setModal('none')}>
        <form onSubmit={e => { e.preventDefault(); updateMut.mutate(); }} className="space-y-4">
          <Field label="Name" required><Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required /></Field>
          <Field label="Category" required>
            <Select value={form.categoryId} onChange={e => setForm(f => ({ ...f, categoryId: e.target.value }))} required>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </Select>
          </Field>
          <Field label="Description"><Textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={2} /></Field>
          <Field label="Price" required><Input type="number" step="0.01" min="0" value={form.price} onChange={e => setForm(f => ({ ...f, price: +e.target.value }))} required /></Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
            <Button type="submit" loading={updateMut.isPending}>Update</Button>
          </div>
        </form>
      </Modal>

      {/* Stock Modal */}
      <Modal title="Adjust Stock" open={modal === 'stock'} onClose={() => setModal('none')} size="sm">
        <p className="text-sm text-slate-600 mb-4">Current stock for <strong>{target?.name}</strong>: <strong>{target?.stockQuantity}</strong></p>
        <form onSubmit={e => { e.preventDefault(); stockMut.mutate(); }} className="space-y-4">
          <Field label="Adjustment (positive to add, negative to remove)" required>
            <Input type="number" value={stockQty} onChange={e => setStockQty(+e.target.value)} placeholder="e.g. 10 or -5" />
          </Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
            <Button type="submit" loading={stockMut.isPending}>Apply</Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirm */}
      <Modal title="Delete Product" open={modal === 'delete'} onClose={() => setModal('none')} size="sm">
        <p className="text-slate-600 text-sm">Are you sure you want to delete <strong>{target?.name}</strong>?</p>
        <div className="flex justify-end gap-2 pt-4">
          <Button variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
          <Button variant="danger" loading={deleteMut.isPending} onClick={() => deleteMut.mutate(target!.id)}>Delete</Button>
        </div>
      </Modal>
    </div>
  );
}
