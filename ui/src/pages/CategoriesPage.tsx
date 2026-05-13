import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { catalogApi } from '../api/catalog';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Textarea, Button } from '../components/FormField';
import { Modal } from '../components/Modal';
import { EmptyState } from '../components/EmptyState';
import { ActiveBadge } from '../components/Badge';
import { PageSpinner } from '../components/Spinner';
import { Tag, Plus, Pencil, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../api/client';
import type { CategoryDto } from '../types';

export default function CategoriesPage() {
  const { tenantId } = useAuth();
  const qc = useQueryClient();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<CategoryDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CategoryDto | null>(null);

  const [form, setForm] = useState({ name: '', description: '' });

  const { data: categories = [], isLoading } = useQuery({
    queryKey: ['categories'],
    queryFn: catalogApi.getCategories,
  });

  const createMut = useMutation({
    mutationFn: () => catalogApi.createCategory({ tenantId, name: form.name, description: form.description || undefined }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['categories'] }); setCreateOpen(false); setForm({ name: '', description: '' }); toast.success('Category created'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const updateMut = useMutation({
    mutationFn: () => catalogApi.updateCategory(editTarget!.id, { name: form.name, description: form.description || undefined }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['categories'] }); setEditTarget(null); toast.success('Category updated'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => catalogApi.deleteCategory(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['categories'] }); setDeleteTarget(null); toast.success('Category deleted'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const openEdit = (cat: CategoryDto) => { setEditTarget(cat); setForm({ name: cat.name, description: cat.description ?? '' }); };
  const openCreate = () => { setForm({ name: '', description: '' }); setCreateOpen(true); };

  if (isLoading) return <PageSpinner />;

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Categories</h1>
          <p className="text-slate-500 text-sm mt-0.5">{categories.length} categories</p>
        </div>
        <Button onClick={openCreate} size="sm"><Plus size={14} /> Add Category</Button>
      </div>

      {categories.length === 0 ? (
        <EmptyState icon={Tag} title="No categories" description="Create your first product category." action={<Button onClick={openCreate}><Plus size={14} /> Add Category</Button>} />
      ) : (
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="px-5 py-3 text-left font-medium text-slate-600">Name</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Slug</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Description</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Status</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Created</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {categories.map(cat => (
                <tr key={cat.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-5 py-3 font-medium text-slate-800">{cat.name}</td>
                  <td className="px-5 py-3 text-slate-500 font-mono text-xs">{cat.slug}</td>
                  <td className="px-5 py-3 text-slate-600 max-w-xs truncate">{cat.description ?? '—'}</td>
                  <td className="px-5 py-3"><ActiveBadge active={cat.isActive} /></td>
                  <td className="px-5 py-3 text-slate-500">{new Date(cat.createdAt).toLocaleDateString()}</td>
                  <td className="px-5 py-3">
                    <div className="flex items-center gap-1 justify-end">
                      <button onClick={() => openEdit(cat)} className="p-1.5 rounded hover:bg-slate-100 text-slate-500 hover:text-slate-700"><Pencil size={14} /></button>
                      <button onClick={() => setDeleteTarget(cat)} className="p-1.5 rounded hover:bg-red-50 text-slate-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create Modal */}
      <Modal title="Add Category" open={createOpen} onClose={() => setCreateOpen(false)} size="sm">
        <form onSubmit={e => { e.preventDefault(); createMut.mutate(); }} className="space-y-4">
          <Field label="Name" required><Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required /></Field>
          <Field label="Description"><Textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={3} /></Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setCreateOpen(false)}>Cancel</Button>
            <Button type="submit" loading={createMut.isPending}>Create</Button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal title="Edit Category" open={!!editTarget} onClose={() => setEditTarget(null)} size="sm">
        <form onSubmit={e => { e.preventDefault(); updateMut.mutate(); }} className="space-y-4">
          <Field label="Name" required><Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required /></Field>
          <Field label="Description"><Textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={3} /></Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setEditTarget(null)}>Cancel</Button>
            <Button type="submit" loading={updateMut.isPending}>Update</Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirm */}
      <Modal title="Delete Category" open={!!deleteTarget} onClose={() => setDeleteTarget(null)} size="sm">
        <p className="text-slate-600 text-sm">Are you sure you want to delete <strong>{deleteTarget?.name}</strong>? This action cannot be undone.</p>
        <div className="flex justify-end gap-2 pt-4">
          <Button variant="secondary" onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button variant="danger" loading={deleteMut.isPending} onClick={() => deleteMut.mutate(deleteTarget!.id)}>Delete</Button>
        </div>
      </Modal>
    </div>
  );
}
