export function Spinner({ className = 'h-8 w-8' }: { className?: string }) {
  return (
    <div className={`animate-spin rounded-full border-2 border-slate-200 border-t-indigo-600 ${className}`} />
  );
}

export function PageSpinner() {
  return (
    <div className="flex items-center justify-center h-64">
      <Spinner className="h-10 w-10" />
    </div>
  );
}
