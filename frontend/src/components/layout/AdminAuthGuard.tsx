'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useAuthStore } from '@/store/auth';

export default function AdminAuthGuard({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuthStore();
  const router = useRouter();
  const pathname = usePathname();
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Get token from localStorage directly to handle initial load
    const storedToken = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
    const isAuth = isAuthenticated || !!storedToken;

    // If not authenticated, redirect to login
    if (!isAuth) {
      router.push('/login');
      setIsLoading(false);
      return;
    }

    // If authenticated but not admin, redirect to home
    if (user && user.role !== 'Admin') {
      router.push('/');
      setIsLoading(false);
      return;
    }

    // If authenticated as admin, allow access
    setIsLoading(false);
  }, [user, isAuthenticated, router]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-900">
        <div className="text-white text-xl">Loading...</div>
      </div>
    );
  }

  return <>{children}</>;
}