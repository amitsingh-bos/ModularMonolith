import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authApi } from '../api/auth';
import { useAuth } from '../contexts/AuthContext';
import { Field, Select, Button } from '../components/FormField';
import { Modal } from '../components/Modal';
import { EmptyState } from '../components/EmptyState';
import { ActiveBadge } from '../components/Badge';
import { PageSpinner } from '../components/Spinner';
import { Users, UserPlus } from 'lucide-react';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../api/client';
import type { UserDto } from '../types';

export default function UsersPage() {
  const { tenantId } = useAuth();
  const qc = useQueryClient();
  const [assignTarget, setAssignTarget] = useState<UserDto | null>(null);
  const [selectedRoleId, setSelectedRoleId] = useState('');

  const { data: users = [], isLoading } = useQuery({
    queryKey: ['users', tenantId],
    queryFn: () => authApi.getUsers({ tenantId }),
  });

  const { data: roles = [] } = useQuery({
    queryKey: ['roles'],
    queryFn: authApi.getRoles,
  });

  const assignMut = useMutation({
    mutationFn: () => authApi.assignRole(assignTarget!.id, { userId: assignTarget!.id, roleId: selectedRoleId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['users'] }); setAssignTarget(null); toast.success('Role assigned'); },
    onError: (e) => toast.error(getErrorMessage(e)),
  });

  if (isLoading) return <PageSpinner />;

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Users</h1>
        <p className="text-slate-500 text-sm mt-0.5">{users.length} users in tenant <strong>{tenantId}</strong></p>
      </div>

      {users.length === 0 ? (
        <EmptyState icon={Users} title="No users" description="Users will appear here after registration." />
      ) : (
        <div className="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="px-5 py-3 text-left font-medium text-slate-600">Name</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Email</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Roles</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Verified</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Status</th>
                <th className="px-5 py-3 text-left font-medium text-slate-600">Last Login</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {users.map(user => (
                <tr key={user.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-5 py-3 font-medium text-slate-800">{user.firstName} {user.lastName}</td>
                  <td className="px-5 py-3 text-slate-600">{user.email}</td>
                  <td className="px-5 py-3">
                    {user.roles.length > 0 ? (
                      <div className="flex flex-wrap gap-1">
                        {user.roles.map(r => (
                          <span key={r} className="px-2 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800">{r}</span>
                        ))}
                      </div>
                    ) : <span className="text-slate-400 text-xs">No roles</span>}
                  </td>
                  <td className="px-5 py-3">
                    <span className={`text-xs font-medium ${user.isEmailVerified ? 'text-green-600' : 'text-amber-600'}`}>
                      {user.isEmailVerified ? 'Verified' : 'Unverified'}
                    </span>
                  </td>
                  <td className="px-5 py-3"><ActiveBadge active={user.isActive} /></td>
                  <td className="px-5 py-3 text-slate-500 text-xs">
                    {user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleString() : 'Never'}
                  </td>
                  <td className="px-5 py-3">
                    <button
                      onClick={() => { setAssignTarget(user); setSelectedRoleId(roles[0]?.id ?? ''); }}
                      className="flex items-center gap-1 text-xs text-indigo-600 hover:text-indigo-800 font-medium"
                    >
                      <UserPlus size={12} /> Assign Role
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Assign Role Modal */}
      <Modal title={`Assign Role — ${assignTarget?.email}`} open={!!assignTarget} onClose={() => setAssignTarget(null)} size="sm">
        <form onSubmit={e => { e.preventDefault(); assignMut.mutate(); }} className="space-y-4">
          <Field label="Role" required>
            <Select value={selectedRoleId} onChange={e => setSelectedRoleId(e.target.value)} required>
              <option value="">Select a role</option>
              {roles.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
            </Select>
          </Field>
          <div className="flex justify-end gap-2 pt-1">
            <Button type="button" variant="secondary" onClick={() => setAssignTarget(null)}>Cancel</Button>
            <Button type="submit" loading={assignMut.isPending}>Assign</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
