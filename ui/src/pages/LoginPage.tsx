import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Button } from '../components/FormField';
import toast from 'react-hot-toast';
import { Info, ShieldCheck } from 'lucide-react';

type Tab = 'login' | 'register';
type PageStep = 'auth' | 'twoFactor';

const DEMO_TENANT_ID = '00000000-0000-0000-0000-000000000001';

export default function LoginPage() {
  const { login, register, verifyTwoFactor } = useAuth();
  const navigate = useNavigate();
  const [tab, setTab] = useState<Tab>('login');
  const [pageStep, setPageStep] = useState<PageStep>('auth');
  const [loading, setLoading] = useState(false);

  const [loginForm, setLoginForm] = useState({
    tenantId: DEMO_TENANT_ID,
    email: 'admin@demo.com',
    password: 'Admin@1234',
  });
  const [regForm, setRegForm] = useState({
    tenantId: DEMO_TENANT_ID,
    email: '',
    password: '',
    firstName: '',
    lastName: '',
  });

  // 2FA challenge state
  const [twoFactorChallenge, setTwoFactorChallenge] = useState<{
    twoFactorToken: string;
    method: string;
    tenantId: string;
  } | null>(null);
  const [twoFactorCode, setTwoFactorCode] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const result = await login(loginForm);
      if (result.requiresTwoFactor) {
        setTwoFactorChallenge({
          twoFactorToken: result.twoFactorToken!,
          method: result.twoFactorMethod!,
          tenantId: loginForm.tenantId,
        });
        setPageStep('twoFactor');
        toast.success('Enter your verification code to continue.');
      } else {
        toast.success('Welcome back!');
        navigate('/');
      }
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await register(regForm);
      toast.success('Account created! Welcome.');
      navigate('/');
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  const handleVerify2fa = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!twoFactorChallenge) return;
    setLoading(true);
    try {
      await verifyTwoFactor(twoFactorChallenge.twoFactorToken, twoFactorCode, twoFactorChallenge.tenantId);
      toast.success('Welcome back!');
      navigate('/');
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Invalid or expired code');
    } finally {
      setLoading(false);
    }
  };

  const methodLabel = (method: string) => {
    if (method === 'GoogleAuthenticator') return 'authenticator app';
    if (method === 'Email') return 'email';
    return 'phone';
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex w-14 h-14 rounded-2xl bg-gradient-to-br from-indigo-500 to-purple-600 items-center justify-center mb-4">
            <span className="text-white font-bold text-2xl">M</span>
          </div>
          <h1 className="text-3xl font-bold text-white">ModularMonolith</h1>
          <p className="text-slate-400 mt-1 text-sm">Demo Dashboard</p>
        </div>

        <div className="bg-white rounded-2xl shadow-2xl overflow-hidden">
          {pageStep === 'twoFactor' ? (
            /* ── 2FA challenge step ── */
            <div className="p-6">
              <div className="text-center mb-6">
                <div className="inline-flex w-12 h-12 rounded-full bg-indigo-100 items-center justify-center mb-3">
                  <ShieldCheck size={22} className="text-indigo-600" />
                </div>
                <h2 className="text-lg font-semibold text-slate-900">Two-Factor Authentication</h2>
                <p className="text-sm text-slate-500 mt-1">
                  Enter the 6-digit code from your{' '}
                  <span className="font-medium">{methodLabel(twoFactorChallenge?.method ?? '')}</span>.
                </p>
              </div>

              <form onSubmit={handleVerify2fa} className="space-y-4">
                <Field label="Verification Code" required>
                  <Input
                    value={twoFactorCode}
                    onChange={e => setTwoFactorCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                    placeholder="000000"
                    maxLength={6}
                    autoFocus
                    className="text-center tracking-[0.4em] text-xl font-mono"
                  />
                </Field>
                <Button type="submit" loading={loading} className="w-full justify-center py-2.5">
                  Verify Code
                </Button>
              </form>

              <button
                type="button"
                onClick={() => { setPageStep('auth'); setTwoFactorCode(''); setTwoFactorChallenge(null); }}
                className="text-xs text-slate-500 hover:text-slate-700 w-full text-center mt-4"
              >
                ← Back to login
              </button>
            </div>
          ) : (
            /* ── Login / Register tabs ── */
            <>
              <div className="flex border-b border-slate-200">
                {(['register', 'login'] as Tab[]).map(t => (
                  <button
                    key={t}
                    onClick={() => setTab(t)}
                    className={`flex-1 py-3.5 text-sm font-medium transition-colors
                      ${tab === t ? 'text-indigo-600 border-b-2 border-indigo-600 bg-indigo-50/50' : 'text-slate-500 hover:text-slate-700'}`}
                  >
                    {t === 'login' ? 'Sign In' : 'Register'}
                  </button>
                ))}
              </div>

              <div className="p-6">
                <div className="flex items-start gap-2 bg-indigo-50 border border-indigo-200 rounded-lg px-3 py-2.5 mb-4">
                  <Info size={14} className="text-indigo-500 mt-0.5 flex-shrink-0" />
                  <p className="text-xs text-indigo-700">
                    Admin credentials are <strong>pre-seeded</strong> — just click Sign In. Or register a new account on the Register tab.
                  </p>
                </div>

                {tab === 'register' ? (
                  <form onSubmit={handleRegister} className="space-y-4">
                    <Field label="Tenant ID" required>
                      <Input
                        value={regForm.tenantId}
                        onChange={e => setRegForm(f => ({ ...f, tenantId: e.target.value }))}
                        required
                      />
                    </Field>
                    <div className="grid grid-cols-2 gap-3">
                      <Field label="First Name" required>
                        <Input
                          value={regForm.firstName}
                          onChange={e => setRegForm(f => ({ ...f, firstName: e.target.value }))}
                          placeholder="John"
                          required
                        />
                      </Field>
                      <Field label="Last Name" required>
                        <Input
                          value={regForm.lastName}
                          onChange={e => setRegForm(f => ({ ...f, lastName: e.target.value }))}
                          placeholder="Doe"
                          required
                        />
                      </Field>
                    </div>
                    <Field label="Email" required>
                      <Input
                        type="email"
                        value={regForm.email}
                        onChange={e => setRegForm(f => ({ ...f, email: e.target.value }))}
                        placeholder="you@example.com"
                        required
                      />
                    </Field>
                    <Field label="Password" required>
                      <Input
                        type="password"
                        value={regForm.password}
                        onChange={e => setRegForm(f => ({ ...f, password: e.target.value }))}
                        placeholder="Min 8 chars, upper + lower + digit + symbol"
                        required
                      />
                    </Field>
                    <div className="pt-1">
                      <Button type="submit" loading={loading} className="w-full justify-center py-2.5">
                        Create Account
                      </Button>
                    </div>
                  </form>
                ) : (
                  <form onSubmit={handleLogin} className="space-y-4">
                    <Field label="Tenant ID" required>
                      <Input
                        value={loginForm.tenantId}
                        onChange={e => setLoginForm(f => ({ ...f, tenantId: e.target.value }))}
                        required
                      />
                    </Field>
                    <Field label="Email" required>
                      <Input
                        type="email"
                        value={loginForm.email}
                        onChange={e => setLoginForm(f => ({ ...f, email: e.target.value }))}
                        placeholder="you@example.com"
                        required
                      />
                    </Field>
                    <Field label="Password" required>
                      <Input
                        type="password"
                        value={loginForm.password}
                        onChange={e => setLoginForm(f => ({ ...f, password: e.target.value }))}
                        placeholder="••••••••"
                        required
                      />
                    </Field>
                    <div className="pt-1">
                      <Button type="submit" loading={loading} className="w-full justify-center py-2.5">
                        Sign In
                      </Button>
                    </div>
                  </form>
                )}
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
