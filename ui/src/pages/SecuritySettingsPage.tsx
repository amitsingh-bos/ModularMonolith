import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authApi } from '../api/auth';
import { Field, Input, Button } from '../components/FormField';
import { getErrorMessage } from '../api/client';
import toast from 'react-hot-toast';
import { Shield, ShieldCheck, ShieldOff, Copy, Smartphone, Mail, MessageSquare } from 'lucide-react';
import type { Setup2FaResponse } from '../types';

type Method = 'GoogleAuthenticator' | 'Email' | 'Sms';
type Step = 'idle' | 'confirm' | 'disable';

const methodLabels: Record<Method, string> = {
  GoogleAuthenticator: 'Google Authenticator',
  Email: 'Email OTP',
  Sms: 'SMS OTP',
};

const methodIcons = {
  GoogleAuthenticator: Smartphone,
  Email: Mail,
  Sms: MessageSquare,
};

export default function SecuritySettingsPage() {
  const qc = useQueryClient();
  const [step, setStep] = useState<Step>('idle');
  const [method, setMethod] = useState<Method>('GoogleAuthenticator');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [code, setCode] = useState('');
  const [setupResult, setSetupResult] = useState<Setup2FaResponse | null>(null);

  const { data: status, isLoading } = useQuery({
    queryKey: ['2fa-status'],
    queryFn: () => authApi.get2faStatus().then(r => r.data),
  });

  const setupMutation = useMutation({
    mutationFn: () =>
      authApi.setup2fa({ method, phoneNumber: method === 'Sms' ? phoneNumber : undefined }),
    onSuccess: res => {
      setSetupResult(res.data);
      setStep('confirm');
    },
    onError: err => toast.error(getErrorMessage(err)),
  });

  const confirmMutation = useMutation({
    mutationFn: () => authApi.confirm2faSetup({ code }),
    onSuccess: () => {
      toast.success('Two-factor authentication enabled!');
      qc.invalidateQueries({ queryKey: ['2fa-status'] });
      setStep('idle');
      setCode('');
      setSetupResult(null);
    },
    onError: err => toast.error(getErrorMessage(err)),
  });

  const disableMutation = useMutation({
    mutationFn: () => authApi.disable2fa({ code }),
    onSuccess: () => {
      toast.success('Two-factor authentication disabled.');
      qc.invalidateQueries({ queryKey: ['2fa-status'] });
      setStep('idle');
      setCode('');
    },
    onError: err => toast.error(getErrorMessage(err)),
  });

  const copyToClipboard = (text: string, label: string) => {
    navigator.clipboard.writeText(text).then(() => toast.success(`${label} copied!`));
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-40 text-slate-400 text-sm">
        Loading…
      </div>
    );
  }

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold text-slate-900 mb-1">Security Settings</h1>
      <p className="text-slate-500 text-sm mb-6">Manage two-factor authentication for your account.</p>

      <div className="bg-white rounded-xl border border-slate-200 divide-y divide-slate-100">
        {/* Status header */}
        <div className="flex items-center gap-4 p-6">
          {status?.enabled ? (
            <div className="w-10 h-10 rounded-full bg-green-100 flex items-center justify-center flex-shrink-0">
              <ShieldCheck size={20} className="text-green-600" />
            </div>
          ) : (
            <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center flex-shrink-0">
              <Shield size={20} className="text-slate-400" />
            </div>
          )}
          <div>
            <p className="font-semibold text-slate-900">
              Two-factor authentication is{' '}
              <span className={status?.enabled ? 'text-green-600' : 'text-slate-500'}>
                {status?.enabled ? 'enabled' : 'disabled'}
              </span>
            </p>
            {status?.enabled && status.method && (
              <p className="text-sm text-slate-500 mt-0.5">
                Method: {methodLabels[status.method as Method] ?? status.method}
              </p>
            )}
          </div>
        </div>

        {/* Enable flow — method selection */}
        {!status?.enabled && step === 'idle' && (
          <div className="p-6 space-y-5">
            <div>
              <p className="text-sm font-medium text-slate-700 mb-3">Choose an authentication method:</p>
              <div className="grid grid-cols-3 gap-3">
                {(['GoogleAuthenticator', 'Email', 'Sms'] as Method[]).map(m => {
                  const Icon = methodIcons[m];
                  return (
                    <button
                      key={m}
                      onClick={() => setMethod(m)}
                      className={`flex flex-col items-center gap-2.5 p-4 rounded-lg border-2 text-sm font-medium transition-colors
                        ${method === m
                          ? 'border-indigo-500 bg-indigo-50 text-indigo-700'
                          : 'border-slate-200 text-slate-600 hover:border-slate-300 hover:bg-slate-50'
                        }`}
                    >
                      <Icon size={20} />
                      <span className="text-xs leading-tight text-center">{methodLabels[m]}</span>
                    </button>
                  );
                })}
              </div>
            </div>

            {method === 'Sms' && (
              <Field label="Phone Number (E.164 format, e.g. +14155552671)" required>
                <Input
                  value={phoneNumber}
                  onChange={e => setPhoneNumber(e.target.value)}
                  placeholder="+14155552671"
                />
              </Field>
            )}

            <Button onClick={() => setupMutation.mutate()} loading={setupMutation.isPending}>
              Start Setup
            </Button>
          </div>
        )}

        {/* Enable flow — confirmation */}
        {!status?.enabled && step === 'confirm' && setupResult && (
          <div className="p-6 space-y-5">
            {setupResult.method === 'GoogleAuthenticator' ? (
              <div className="space-y-4">
                <p className="text-sm font-medium text-slate-700">
                  1. Open Google Authenticator, tap <strong>+</strong> → <strong>Enter a setup key</strong>, and paste the secret key below.
                </p>
                <div className="bg-slate-50 rounded-lg p-4 space-y-3">
                  <div>
                    <p className="text-xs font-medium text-slate-500 mb-1.5">Secret Key (manual entry):</p>
                    <div className="flex items-center gap-2">
                      <code className="flex-1 text-sm bg-white border border-slate-200 rounded-lg px-3 py-2 font-mono break-all text-slate-800">
                        {setupResult.secretKey}
                      </code>
                      <button
                        onClick={() => copyToClipboard(setupResult.secretKey!, 'Secret key')}
                        className="p-2 rounded-lg hover:bg-slate-200 text-slate-500 transition-colors"
                        title="Copy secret key"
                      >
                        <Copy size={14} />
                      </button>
                    </div>
                  </div>
                  <div>
                    <p className="text-xs font-medium text-slate-500 mb-1.5">Or copy the QR URI for a QR generator:</p>
                    <div className="flex items-start gap-2">
                      <code className="flex-1 text-xs bg-white border border-slate-200 rounded-lg px-3 py-2 font-mono break-all text-indigo-600">
                        {setupResult.qrCodeUri}
                      </code>
                      <button
                        onClick={() => copyToClipboard(setupResult.qrCodeUri!, 'QR URI')}
                        className="p-2 rounded-lg hover:bg-slate-200 text-slate-500 transition-colors flex-shrink-0"
                        title="Copy QR URI"
                      >
                        <Copy size={14} />
                      </button>
                    </div>
                  </div>
                </div>
                <p className="text-sm font-medium text-slate-700">
                  2. Enter the 6-digit code from your authenticator app:
                </p>
              </div>
            ) : (
              <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm text-blue-700">
                {setupResult.message ?? 'A verification code has been sent. Enter it below to confirm setup.'}
              </div>
            )}

            <Field label="Verification Code" required>
              <Input
                value={code}
                onChange={e => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="000000"
                maxLength={6}
                autoFocus
                className="tracking-[0.3em] text-center font-mono text-lg"
              />
            </Field>

            <div className="flex gap-3">
              <Button onClick={() => confirmMutation.mutate()} loading={confirmMutation.isPending}>
                Confirm &amp; Enable
              </Button>
              <Button
                variant="secondary"
                onClick={() => { setStep('idle'); setCode(''); setSetupResult(null); }}
              >
                Cancel
              </Button>
            </div>
          </div>
        )}

        {/* Disable flow — action button */}
        {status?.enabled && step === 'idle' && (
          <div className="p-6">
            <Button variant="danger" onClick={() => setStep('disable')}>
              <ShieldOff size={15} />
              Disable 2FA
            </Button>
          </div>
        )}

        {/* Disable flow — confirmation */}
        {status?.enabled && step === 'disable' && (
          <div className="p-6 space-y-4">
            <p className="text-sm text-slate-600">
              Enter your current 2FA code to confirm disabling two-factor authentication:
            </p>
            <Field label="Verification Code" required>
              <Input
                value={code}
                onChange={e => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="000000"
                maxLength={6}
                autoFocus
                className="tracking-[0.3em] text-center font-mono text-lg"
              />
            </Field>
            <div className="flex gap-3">
              <Button variant="danger" onClick={() => disableMutation.mutate()} loading={disableMutation.isPending}>
                Confirm Disable
              </Button>
              <Button variant="secondary" onClick={() => { setStep('idle'); setCode(''); }}>
                Cancel
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
