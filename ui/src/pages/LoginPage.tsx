import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Field, Input, Button } from '../components/FormField';
import toast from 'react-hot-toast';
import { Info, ShieldCheck, Mail, KeyRound, Smartphone, ArrowLeft } from 'lucide-react';
import { authApi } from '../api/auth';
import { getErrorMessage } from '../api/client';

type Tab = 'login' | 'register';
type PageStep = 'auth' | 'twoFactor' | 'forgot' | 'emailReset' | 'totpReset';

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

  // Forgot / reset password state
  const [forgotForm, setForgotForm] = useState({ tenantId: DEMO_TENANT_ID, email: '' });
  const [emailResetForm, setEmailResetForm] = useState({ resetToken: '', newPassword: '', confirmPassword: '' });
  const [totpResetForm, setTotpResetForm] = useState({ totpCode: '', newPassword: '', confirmPassword: '' });
  const [resetContext, setResetContext] = useState<{ tenantId: string; email: string; stepUpToken?: string }>({
    tenantId: DEMO_TENANT_ID,
    email: '',
  });

  // ── Login / Register ──────────────────────────────────────────────────────

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

  // ── Forgot password ───────────────────────────────────────────────────────

  const handleForgotByEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authApi.forgotPassword({ ...forgotForm, method: 'email' });
      setResetContext({ tenantId: forgotForm.tenantId, email: forgotForm.email });
      setEmailResetForm({ resetToken: '', newPassword: '', confirmPassword: '' });
      setPageStep('emailReset');
      toast.success('If that account exists, a reset token has been sent to your email.');
    } catch (err: unknown) {
      toast.error(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleForgotByTotp = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await authApi.forgotPassword({ ...forgotForm, method: 'totp' });
      const stepUpToken = res.data?.stepUpToken;
      if (!stepUpToken) {
        toast.error('No TOTP authenticator is linked to this account, or the account does not exist. Try the email method instead.');
        return;
      }
      setResetContext({ tenantId: forgotForm.tenantId, email: forgotForm.email, stepUpToken });
      setTotpResetForm({ totpCode: '', newPassword: '', confirmPassword: '' });
      setPageStep('totpReset');
    } catch (err: unknown) {
      toast.error(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  // ── Reset password ────────────────────────────────────────────────────────

  const handleEmailReset = async (e: React.FormEvent) => {
    e.preventDefault();
    if (emailResetForm.newPassword !== emailResetForm.confirmPassword) {
      toast.error('Passwords do not match.');
      return;
    }
    setLoading(true);
    try {
      await authApi.resetPassword({
        method: 'email',
        tenantId: resetContext.tenantId,
        email: resetContext.email,
        resetToken: emailResetForm.resetToken,
        newPassword: emailResetForm.newPassword,
      });
      toast.success('Password reset successfully. Please sign in with your new password.');
      goToLogin();
    } catch (err: unknown) {
      toast.error(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleTotpReset = async (e: React.FormEvent) => {
    e.preventDefault();
    if (totpResetForm.newPassword !== totpResetForm.confirmPassword) {
      toast.error('Passwords do not match.');
      return;
    }
    setLoading(true);
    try {
      await authApi.resetPassword({
        method: 'totp',
        stepUpToken: resetContext.stepUpToken,
        totpCode: totpResetForm.totpCode,
        newPassword: totpResetForm.newPassword,
      });
      toast.success('Password reset successfully. Please sign in with your new password.');
      goToLogin();
    } catch (err: unknown) {
      toast.error(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  // ── Helpers ───────────────────────────────────────────────────────────────

  const goToLogin = () => {
    setPageStep('auth');
    setTab('login');
    setForgotForm({ tenantId: DEMO_TENANT_ID, email: '' });
    setTwoFactorCode('');
    setTwoFactorChallenge(null);
  };

  const openForgot = () => {
    setForgotForm({ tenantId: loginForm.tenantId, email: loginForm.email });
    setPageStep('forgot');
  };

  const methodLabel = (method: string) => {
    if (method === 'GoogleAuthenticator') return 'authenticator app';
    if (method === 'Email') return 'email';
    return 'phone';
  };

  // ── Render ────────────────────────────────────────────────────────────────

  const renderStep = () => {
    // ── 2FA verification ──
    if (pageStep === 'twoFactor') {
      return (
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
          <BackLink onClick={goToLogin} label="Back to login" />
        </div>
      );
    }

    // ── Forgot password — choose method ──
    if (pageStep === 'forgot') {
      return (
        <div className="p-6">
          <div className="text-center mb-6">
            <div className="inline-flex w-12 h-12 rounded-full bg-amber-100 items-center justify-center mb-3">
              <KeyRound size={20} className="text-amber-600" />
            </div>
            <h2 className="text-lg font-semibold text-slate-900">Reset Password</h2>
            <p className="text-sm text-slate-500 mt-1">Enter your account details, then choose how to reset.</p>
          </div>

          <div className="space-y-4">
            <Field label="Tenant ID" required>
              <Input
                value={forgotForm.tenantId}
                onChange={e => setForgotForm(f => ({ ...f, tenantId: e.target.value }))}
                required
              />
            </Field>
            <Field label="Email" required>
              <Input
                type="email"
                value={forgotForm.email}
                onChange={e => setForgotForm(f => ({ ...f, email: e.target.value }))}
                placeholder="you@example.com"
                required
                autoFocus
              />
            </Field>

            <div className="pt-1 grid grid-cols-2 gap-3">
              <button
                type="button"
                onClick={handleForgotByEmail}
                disabled={loading || !forgotForm.email || !forgotForm.tenantId}
                className="flex flex-col items-center gap-2 rounded-xl border-2 border-slate-200 hover:border-indigo-400 hover:bg-indigo-50 p-4 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
              >
                <Mail size={22} className="text-indigo-500" />
                <span className="text-xs font-medium text-slate-700 leading-tight text-center">
                  Send Reset<br />Email
                </span>
              </button>
              <button
                type="button"
                onClick={handleForgotByTotp}
                disabled={loading || !forgotForm.email || !forgotForm.tenantId}
                className="flex flex-col items-center gap-2 rounded-xl border-2 border-slate-200 hover:border-indigo-400 hover:bg-indigo-50 p-4 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
              >
                <Smartphone size={22} className="text-indigo-500" />
                <span className="text-xs font-medium text-slate-700 leading-tight text-center">
                  Use Authenticator<br />App
                </span>
              </button>
            </div>
          </div>

          <BackLink onClick={goToLogin} label="Back to login" />
        </div>
      );
    }

    // ── Email reset — enter token + new password ──
    if (pageStep === 'emailReset') {
      return (
        <div className="p-6">
          <div className="text-center mb-6">
            <div className="inline-flex w-12 h-12 rounded-full bg-green-100 items-center justify-center mb-3">
              <Mail size={20} className="text-green-600" />
            </div>
            <h2 className="text-lg font-semibold text-slate-900">Check Your Email</h2>
            <p className="text-sm text-slate-500 mt-1">
              We sent a reset token to <span className="font-medium text-slate-700">{resetContext.email}</span>.
              Paste it below along with your new password.
            </p>
          </div>

          <form onSubmit={handleEmailReset} className="space-y-4">
            <Field label="Reset Token" required>
              <Input
                value={emailResetForm.resetToken}
                onChange={e => setEmailResetForm(f => ({ ...f, resetToken: e.target.value.trim() }))}
                placeholder="Paste token from email"
                autoFocus
                className="font-mono text-xs"
              />
            </Field>
            <Field label="New Password" required>
              <Input
                type="password"
                value={emailResetForm.newPassword}
                onChange={e => setEmailResetForm(f => ({ ...f, newPassword: e.target.value }))}
                placeholder="Min 8 chars, upper + lower + digit + symbol"
              />
            </Field>
            <Field label="Confirm New Password" required>
              <Input
                type="password"
                value={emailResetForm.confirmPassword}
                onChange={e => setEmailResetForm(f => ({ ...f, confirmPassword: e.target.value }))}
                placeholder="Repeat new password"
              />
            </Field>
            <div className="pt-1">
              <Button type="submit" loading={loading} className="w-full justify-center py-2.5">
                Reset Password
              </Button>
            </div>
          </form>

          <BackLink onClick={() => setPageStep('forgot')} label="Back" />
        </div>
      );
    }

    // ── TOTP reset — enter code + new password ──
    if (pageStep === 'totpReset') {
      return (
        <div className="p-6">
          <div className="text-center mb-6">
            <div className="inline-flex w-12 h-12 rounded-full bg-indigo-100 items-center justify-center mb-3">
              <Smartphone size={20} className="text-indigo-600" />
            </div>
            <h2 className="text-lg font-semibold text-slate-900">Authenticator Verification</h2>
            <p className="text-sm text-slate-500 mt-1">
              Enter the current 6-digit code from your authenticator app and set a new password.
            </p>
          </div>

          <form onSubmit={handleTotpReset} className="space-y-4">
            <Field label="Authenticator Code" required>
              <Input
                value={totpResetForm.totpCode}
                onChange={e => setTotpResetForm(f => ({ ...f, totpCode: e.target.value.replace(/\D/g, '').slice(0, 6) }))}
                placeholder="000000"
                maxLength={6}
                autoFocus
                className="text-center tracking-[0.4em] text-xl font-mono"
              />
            </Field>
            <Field label="New Password" required>
              <Input
                type="password"
                value={totpResetForm.newPassword}
                onChange={e => setTotpResetForm(f => ({ ...f, newPassword: e.target.value }))}
                placeholder="Min 8 chars, upper + lower + digit + symbol"
              />
            </Field>
            <Field label="Confirm New Password" required>
              <Input
                type="password"
                value={totpResetForm.confirmPassword}
                onChange={e => setTotpResetForm(f => ({ ...f, confirmPassword: e.target.value }))}
                placeholder="Repeat new password"
              />
            </Field>
            <div className="pt-1">
              <Button type="submit" loading={loading} className="w-full justify-center py-2.5">
                Reset Password
              </Button>
            </div>
          </form>

          <BackLink onClick={() => setPageStep('forgot')} label="Back" />
        </div>
      );
    }

    // ── Default: Login / Register tabs ──
    return (
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
              <div className="pt-1 space-y-3">
                <Button type="submit" loading={loading} className="w-full justify-center py-2.5">
                  Sign In
                </Button>
                <div className="text-center">
                  <button
                    type="button"
                    onClick={openForgot}
                    className="text-xs text-indigo-600 hover:text-indigo-800 hover:underline"
                  >
                    Forgot your password?
                  </button>
                </div>
              </div>
            </form>
          )}
        </div>
      </>
    );
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
          {renderStep()}
        </div>
      </div>
    </div>
  );
}

function BackLink({ onClick, label }: { onClick: () => void; label: string }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex items-center gap-1 text-xs text-slate-500 hover:text-slate-700 w-full justify-center mt-4"
    >
      <ArrowLeft size={12} />
      {label}
    </button>
  );
}
