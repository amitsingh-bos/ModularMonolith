import type { InputHTMLAttributes, SelectHTMLAttributes, ReactNode } from 'react';

interface FieldProps {
  label: string;
  error?: string;
  required?: boolean;
  children: ReactNode;
}

export function Field({ label, error, required, children }: FieldProps) {
  return (
    <div className="flex flex-col gap-1">
      <label className="text-sm font-medium text-slate-700">
        {label}{required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      {children}
      {error && <p className="text-xs text-red-600">{error}</p>}
    </div>
  );
}

type InputProps = InputHTMLAttributes<HTMLInputElement> & { error?: boolean };
export function Input({ error, className = '', ...props }: InputProps) {
  return (
    <input
      {...props}
      className={`px-3 py-2 rounded-lg border text-sm transition-colors outline-none
        ${error
          ? 'border-red-400 focus:border-red-500 focus:ring-2 focus:ring-red-200'
          : 'border-slate-300 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100'}
        ${className}`}
    />
  );
}

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & { error?: boolean };
export function Select({ error, className = '', children, ...props }: SelectProps) {
  return (
    <select
      {...props}
      className={`px-3 py-2 rounded-lg border text-sm transition-colors outline-none bg-white
        ${error
          ? 'border-red-400 focus:border-red-500 focus:ring-2 focus:ring-red-200'
          : 'border-slate-300 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100'}
        ${className}`}
    >
      {children}
    </select>
  );
}

export function Textarea({ error, className = '', ...props }: React.TextareaHTMLAttributes<HTMLTextAreaElement> & { error?: boolean }) {
  return (
    <textarea
      {...props}
      className={`px-3 py-2 rounded-lg border text-sm transition-colors outline-none resize-none
        ${error
          ? 'border-red-400 focus:border-red-500 focus:ring-2 focus:ring-red-200'
          : 'border-slate-300 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100'}
        ${className}`}
    />
  );
}

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md';
  loading?: boolean;
}

export function Button({ variant = 'primary', size = 'md', loading, children, className = '', disabled, ...props }: ButtonProps) {
  const base = 'inline-flex items-center gap-2 font-medium rounded-lg transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed';
  const variants = {
    primary:   'bg-indigo-600 hover:bg-indigo-700 text-white',
    secondary: 'bg-white hover:bg-slate-50 text-slate-700 border border-slate-300',
    danger:    'bg-red-600 hover:bg-red-700 text-white',
    ghost:     'hover:bg-slate-100 text-slate-600',
  };
  const sizes = { sm: 'px-3 py-1.5 text-xs', md: 'px-4 py-2 text-sm' };

  return (
    <button
      {...props}
      disabled={disabled || loading}
      className={`${base} ${variants[variant]} ${sizes[size]} ${className}`}
    >
      {loading && <div className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-current border-t-transparent" />}
      {children}
    </button>
  );
}
