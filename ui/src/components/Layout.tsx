import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import {
  LayoutDashboard, Package, Tag, ShoppingCart,
  CreditCard, Users, Shield, LogOut, Menu,
} from 'lucide-react';
import { useState } from 'react';
import type { ReactNode } from 'react';

interface NavItem {
  to: string;
  icon: typeof LayoutDashboard;
  label: string;
  module: string;
}

const navItems: NavItem[] = [
  { to: '/',           icon: LayoutDashboard, label: 'Dashboard',  module: 'dashboard' },
  { to: '/products',   icon: Package,         label: 'Products',   module: 'catalog' },
  { to: '/categories', icon: Tag,             label: 'Categories', module: 'catalog' },
  { to: '/orders',     icon: ShoppingCart,    label: 'Orders',     module: 'orders' },
  { to: '/payments',   icon: CreditCard,      label: 'Payments',   module: 'payments' },
  { to: '/users',      icon: Users,           label: 'Users',      module: 'auth' },
  { to: '/roles',      icon: Shield,          label: 'Roles',      module: 'auth' },
];

const moduleColors: Record<string, string> = {
  dashboard: 'from-indigo-500 to-purple-600',
  catalog:   'from-emerald-500 to-teal-600',
  orders:    'from-orange-500 to-amber-600',
  payments:  'from-blue-500 to-cyan-600',
  auth:      'from-rose-500 to-pink-600',
};

export function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const Sidebar = () => (
    <aside className="flex flex-col h-full bg-slate-900 text-white">
      {/* Logo */}
      <div className="flex items-center gap-3 px-5 py-5 border-b border-slate-700/60">
        <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center flex-shrink-0">
          <span className="text-white font-bold text-sm">M</span>
        </div>
        <div className="min-w-0">
          <p className="font-semibold text-sm truncate">ModularMonolith</p>
          <p className="text-xs text-slate-400 truncate">{user?.tenantId}</p>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-3 py-4 space-y-0.5 overflow-y-auto">
        {navItems.map(({ to, icon: Icon, label, module }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            onClick={() => setSidebarOpen(false)}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all group
               ${isActive
                 ? `bg-white/10 text-white`
                 : 'text-slate-400 hover:text-white hover:bg-white/5'}`
            }
          >
            {({ isActive }) => (
              <>
                <div className={`w-6 h-6 rounded-md flex items-center justify-center flex-shrink-0 transition-all
                  ${isActive ? `bg-gradient-to-br ${moduleColors[module]}` : 'bg-slate-800 group-hover:bg-slate-700'}`}>
                  <Icon size={13} />
                </div>
                {label}
              </>
            )}
          </NavLink>
        ))}
      </nav>

      {/* User */}
      <div className="px-3 py-3 border-t border-slate-700/60">
        <div className="flex items-center gap-3 px-2 py-2 rounded-lg mb-1">
          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-indigo-400 to-purple-500 flex items-center justify-center flex-shrink-0">
            <span className="text-white font-semibold text-xs">
              {user?.email.charAt(0).toUpperCase()}
            </span>
          </div>
          <div className="min-w-0 flex-1">
            <p className="text-sm font-medium text-white truncate">{user?.email}</p>
            <p className="text-xs text-slate-400">{user?.roles.join(', ') || 'User'}</p>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-sm text-slate-400 hover:text-white hover:bg-white/5 transition-colors"
        >
          <LogOut size={14} />
          Sign out
        </button>
      </div>
    </aside>
  );

  return (
    <div className="flex h-screen overflow-hidden bg-slate-50">
      {/* Desktop sidebar */}
      <div className="hidden lg:flex w-60 flex-shrink-0 flex-col">
        <Sidebar />
      </div>

      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div className="lg:hidden fixed inset-0 z-50 flex">
          <div className="fixed inset-0 bg-black/50" onClick={() => setSidebarOpen(false)} />
          <div className="relative w-60 flex flex-col z-10">
            <Sidebar />
          </div>
        </div>
      )}

      {/* Main */}
      <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
        {/* Top bar (mobile) */}
        <header className="lg:hidden flex items-center gap-3 px-4 py-3 bg-white border-b border-slate-200">
          <button onClick={() => setSidebarOpen(true)} className="p-1.5 rounded-lg hover:bg-slate-100">
            <Menu size={20} className="text-slate-600" />
          </button>
          <span className="font-semibold text-slate-800">ModularMonolith</span>
        </header>

        <main className="flex-1 overflow-y-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
