'use client';

import { useState, type FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

export default function ChangePasswordForm() {
  const router = useRouter();
  const logout = useAuthStore(state => state.logout);
  const [form, setForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    if (form.newPassword !== form.confirmPassword) {
      setError('The new passwords must match.');
      return;
    }
    setBusy(true);
    try {
      await api.changePassword(form);
      logout();
      router.push('/login?passwordChanged=1');
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Unable to change your password. Please try again.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <form onSubmit={submit} className="mt-8 max-w-md space-y-4 border-t border-white/10 pt-6">
      <div>
        <h3 className="text-lg font-semibold text-white">Account security</h3>
        <p className="mt-1 text-sm text-gray-400">Choose a unique password of at least 12 characters. Changing it signs you out on all devices.</p>
      </div>
      {(['currentPassword', 'newPassword', 'confirmPassword'] as const).map((key) => (
        <label key={key} className="block text-sm text-gray-300">
          {key === 'currentPassword' ? 'Current password' : key === 'newPassword' ? 'New password' : 'Confirm new password'}
          <input type="password" required minLength={key === 'currentPassword' ? undefined : 12} maxLength={72}
            autoComplete={key === 'currentPassword' ? 'current-password' : 'new-password'} value={form[key]}
            onChange={event => setForm({ ...form, [key]: event.target.value })}
            className="mt-2 w-full rounded-lg border border-gray-600 bg-gray-900 px-4 py-2.5 text-white focus:border-red-400 focus:outline-none" />
        </label>
      ))}
      {error && <p role="alert" className="text-sm text-red-300">{error}</p>}
      <button disabled={busy} className="rounded-lg bg-red-600 px-5 py-2.5 font-semibold text-white hover:bg-red-500 disabled:opacity-50">
        {busy ? 'Changing password…' : 'Change password'}
      </button>
    </form>
  );
}
