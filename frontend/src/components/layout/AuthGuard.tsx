'use client';

import { useEffect, useState, useCallback } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useAuthStore } from '@/store/auth';

// Routes that don't require authentication (public routes)
const PUBLIC_ROUTES = [
  '/',
  '/login',
  '/register',
  '/ott',
  '/cinema',
];

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuthStore();
  const router = useRouter();
  const pathname = usePathname();
  const [isLoading, setIsLoading] = useState(true);

  const checkAuth = useCallback(() => {
    // Skip auth check for admin routes - they're handled by AdminAuthGuard
    if (pathname.startsWith('/admin')) {
      setIsLoading(false);
      return;
    }

    // Get token from localStorage directly to handle initial load
    const storedToken = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
    const isAuth = isAuthenticated || !!storedToken;
    
    // Check route types
    const isPublicRoute = PUBLIC_ROUTES.some(route => {
      if (route === '/') return pathname === '/';
      return pathname.startsWith(route);
    });

    // Check if user is admin
    const isAdmin = user?.role === 'Admin';

    // Administrators also need their account settings and public catalogue.
    if (isAdmin && (pathname === '/login' || pathname === '/register')) {
      router.push('/admin');
      return;
    }

    // If not authenticated and trying to access protected route
    if (!isAuth) {
      // Allow public routes
      if (!isPublicRoute) {
        router.push('/login');
        return;
      }
      setIsLoading(false);
      return;
    }

    // If authenticated user tries to access login/register - redirect to home
    if (pathname === '/login' || pathname === '/register') {
      router.push('/');
      return;
    }

    setIsLoading(false);
  }, [user, isAuthenticated, pathname, router]);

  useEffect(() => {
    // Delay checking to allow store to hydrate
    const timer = setTimeout(() => {
      checkAuth();
    }, 100);

    return () => clearTimeout(timer);
  }, [checkAuth]);

  if (isLoading) {
    return null;
  }

  return <>{children}</>;
}
