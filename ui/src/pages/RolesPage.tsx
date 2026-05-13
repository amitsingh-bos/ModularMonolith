import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authApi } from '../api/auth';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Textarea, Select, Button } from '../components/FormField';
import { Modal } from '../components/Modal';
import { EmptyState } from '../components/EmptyState';
import { PageSpinner } from '../components/Spinner';
import { Shield, Plus, Trash2, KeyRound, X } from 'lucide-react';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../api/client';
import type { RoleDto } from '../types';

type ModalType = 'none' | 'create' | 'permissions' | 'delete';

export default function RolesPage() {
  const { tenantId } = useAuth();
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalType>('none');
  const [selected, setSelected] = useState<RoleDto | null>(null);
  const [form, setForm] = useState({ name: '', description: '' });
  const [addPermId, setAddPermId] = useState('');

  const { data: roles = [], isLoading } = useQuery({
    queryKey: ['roles'],
    queryFn: authApi.getRoles,
  });

  const { data: permissions = [] } = useQuery({
    queryKey: ['permissions'],
    queryFn: authApi.getPermissions,
  });

  const createMut = useMutation({
    mutationFn: () => authApi.createRole({ tenantId, name: form.name, description: form.description || undefined }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['roles'] }); setModal('none'); toast.success('Role created'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => authApi.deleteRole(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['roles'] }); setModal('none'); setSelected(null); toast.success('Role deleted'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const assignPermMut = useMutation({
    mutationFn: () => authApi.assignPermission(selected!.id, { permissionId: addPermId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['roles'] }); toast.success('Permission assigned'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const removePermMut = useMutation({
    mutationFn: ({ roleId, permId }: { roleId: string; permId: string }) => authApi.removePermission(roleId, permId),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['roles'] }); toast.success('Permission removed'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  const openPermissions = (role: RoleDto) => {
    setSelected(role);
    const unassigned = permissions.filter(p => !role.permissions.includes(p.code));
    setAddPermId(unassigned[0]?.id ?? '');
    setModal('permissions');
  };

  // Group permissions by module
  const grouped = permissions.reduce<Record<string, typeof permissions>>((acc, p) => {
    acc[p.module] = [...(acc[p.module] ?? []), p];
    return acc;
  }, {});

  if (isLoading) return <PageSpinner />;

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Roles & Permissions</h1>
          <p className="text-slate-500 text-sm mt-0.5">{roles.length} roles, {permissions.length} permissions</p>
        </div>
        <Button size="sm" onClick={() => { setForm({ name: '', description: '' }); setModal('create'); }}>
          <Plus size={14} /> New Role
        </Button>
      </div>

      {roles.length === 0 ? (
        <EmptyState icon={Shield} title="No roles" description="Create your first role." action={<Button onClick={() => setModal('create')}><Plus size={14} /> New Role</Button>} />
      ) : (
        <div className="grid lg:grid-cols-2 gap-4">
          {roles.map(role => (
            <div key={role.id} className="bg-white rounded-xl border border-slate-200 overflow-hidden">
              <div className="flex items-center justify-between px-5 py-4 border-b border-slate-100">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-lg bg-indigo-100 flex items-center justify-center">
                    <Shield size={14} className="text-indigo-600" />
                  </div>
                  <div>
                    <p className="font-semibold text-slate-800">{role.name}</p>
                    {role.description && <p className="text-xs text-slate-500">{role.description}</p>}
                  </div>
                </div>
                <div className="flex gap-1">
                  <button onClick={() => openPermissions(role)} className="p-1.5 rounded hover:bg-indigo-50 text-slate-400 hover:text-indigo-600" title="Manage permissions">
                    <KeyRound size={14} />
                  </button>
                  <button onClick={() => { setSelected(role); setModal('delete'); }} className="p-1.5 rounded hover:bg-red-50 text-slate-400 hover:text-red-600">
                    <Trash2 size={14} />
                  </button>
                </div>
              </div>
              <div className="px-5 py-3">
                {role.permissions.length === 0 ? (
                  <p className="text-xs text-slate-400">No permissions assigned</p>
                ) : (
                  <div className="flex flex-wrap gap-1.5">
                    {role.permissions.map(code => (
                      <span key={code} className="px-2 py-0.5 rounded text-xs font-mono bg-slate-100 text-slate-700">{code}</span>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* All Permissions Reference */}
      <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
        <div className="px-5 py-4 border-b border-slate-100">
          <h2 className="font-semibold text-slate-800">All System Permissions</h2>
          <p className="text-xs text-slate-500 mt-0.5">12 permissions across 4 modules</p>
        </div>
        <div className="grid lg:grid-cols-2 gap-0 divide-y lg:divide-y-0 lg:divide-x divide-slate-100">
          {Object.entries(grouped).map(([mod, perms]) => (
            <div key={mod} className="p-5">
              <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2">{mod}</p>
              <div className="space-y-1.5">
                {perms.map(p => (
                  <div key={p.id} className="flex items-center gap-2">
                    <span className="font-mono text-xs bg-slate-100 text-slate-700 px-2 py-0.5 rounded">{p.code}</span>
                    <span className="text-xs text-slate-500">{p.description}</span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Create Modal */}
      <Modal title="New Role" open={modal === 'create'} onClose={() => setModal('none')} size="sm">
        <form onSubmit={e => { e.preventDefault(); createMut.mutate(); }} className="space-y-4">
          <Field label="Name" required><Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required /></Field>
          <Field label="Description"><Textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={2} /></Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
            <Button type="submit" loading={createMut.isPending}>Create</Button>
          </div>
        </form>
      </Modal>

      {/* Permissions Modal */}
      {selected && (
        <Modal title={`Permissions — ${selected.name}`} open={modal === 'permissions'} onClose={() => setModal('none')}>
          <div className="space-y-4">
            {/* Assigned */}
            <div>
              <p className="text-sm font-medium text-slate-700 mb-2">Assigned Permissions</p>
              {selected.permissions.length === 0 ? (
                <p className="text-xs text-slate-400">None assigned yet</p>
              ) : (
                <div className="flex flex-wrap gap-2">
                  {selected.permissions.map(code => {
                    const perm = permissions.find(p => p.code === code);
                    return (
                      <div key={code} className="flex items-center gap-1 bg-indigo-50 border border-indigo-200 rounded-lg px-2.5 py-1">
                        <span className="text-xs font-mono text-indigo-800">{code}</span>
                        {perm && (
                          <button
                            onClick={() => removePermMut.mutate({ roleId: selected.id, permId: perm.id })}
                            className="ml-1 text-indigo-400 hover:text-red-500"
                          >
                            <X size={11} />
                          </button>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Add permission */}
            <div className="border-t border-slate-200 pt-4">
              <p className="text-sm font-medium text-slate-700 mb-2">Add Permission</p>
              <div className="flex gap-2">
                <Select value={addPermId} onChange={e => setAddPermId(e.target.value)} className="flex-1">
                  {permissions
                    .filter(p => !selected.permissions.includes(p.code))
                    .map(p => <option key={p.id} value={p.id}>{p.code}</option>)}
                </Select>
                <Button loading={assignPermMut.isPending} onClick={() => assignPermMut.mutate()}>Add</Button>
              </div>
            </div>
          </div>
        </Modal>
      )}

      {/* Delete Confirm */}
      <Modal title="Delete Role" open={modal === 'delete'} onClose={() => setModal('none')} size="sm">
        <p className="text-slate-600 text-sm">Delete role <strong>{selected?.name}</strong>? This cannot be undone.</p>
        <div className="flex justify-end gap-2 pt-4">
          <Button variant="secondary" onClick={() => setModal('none')}>Cancel</Button>
          <Button variant="danger" loading={deleteMut.isPending} onClick={() => deleteMut.mutate(selected!.id)}>Delete</Button>
        </div>
      </Modal>
    </div>
  );
}
