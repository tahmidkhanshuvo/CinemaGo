'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Navbar from '@/components/layout/Navbar';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const router = useRouter();
  const { isAuthenticated, user } = useAuthStore();
  const setAuth = useAuthStore((state) => state.setAuth);

  useEffect(() => {
    // If already authenticated, redirect to appropriate page
    if (isAuthenticated) {
      if (user?.role === 'Admin') {
        router.push('/admin');
      } else {
        router.push('/');
      }
    }
  }, [isAuthenticated, user, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await api.login({ email, password }) as any;
      if (response.success && response.data) {
        setAuth(response.data.user, response.data.token);
        // Redirect admin to dashboard, user to home
        if (response.data.user.role === 'Admin') {
          router.push('/admin');
        } else {
          router.push('/');
        }
      } else {
        setError(response.message || 'Login failed');
      }
    } catch (err: any) {
      setError(err.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  if (isAuthenticated) {
    return null; // Will redirect via useEffect
  }

  return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      
      <div className="flex justify-center items-center min-h-[80vh]">
        <div className="bg-gray-800 p-8 rounded-lg w-full max-w-md">
          <h2 className="text-3xl font-bold text-white mb-6 text-center">Login</h2>
          
          {error && (
            <div className="bg-red-500/20 text-red-400 p-3 rounded mb-4">
              {error}
            </div>
          )}
          
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block text-gray-400 mb-2">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded focus:outline-none focus:ring-2 focus:ring-red-500"
                required
              />
            </div>
            
            <div className="mb-6">
              <label className="block text-gray-400 mb-2">Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded focus:outline-none focus:ring-2 focus:ring-red-500"
                required
              />
            </div>
            
            <button
              type="submit"
              disabled={loading}
              className="w-full py-3 bg-red-600 hover:bg-red-700 text-white font-semibold rounded disabled:opacity-50"
            >
              {loading ? 'Logging in...' : 'Login'}
            </button>
          </form>
          
          <p className="text-gray-400 mt-4 text-center">
            Don't have an account?{' '}
            <Link href="/register" className="text-red-400 hover:underline">
              Register
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}