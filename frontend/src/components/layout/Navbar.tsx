'use client';

import Link from 'next/link';
import { useAuthStore } from '@/store/auth';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

export default function Navbar() {
  const { user, isAuthenticated, logout } = useAuthStore();
  const router = useRouter();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const handleLogout = () => {
    logout();
    router.push('/');
  };

  if (!mounted) return null;

  return (
    <nav className="bg-gray-900 text-white p-4">
      <div className="container mx-auto flex justify-between items-center">
        <Link href="/" className="text-2xl font-bold text-red-500">
          CinemaGo
        </Link>
        
        <div className="flex gap-6">
          <Link href="/ott" className="hover:text-red-400">
            OTT Movies
          </Link>
          <Link href="/cinema" className="hover:text-red-400">
            Cinema
          </Link>
          
          {isAuthenticated ? (
            <>
              {user?.role === 'Admin' && (
                <Link href="/admin" className="hover:text-red-400">
                  Admin
                </Link>
              )}
              <Link href="/profile" className="hover:text-red-400">
                Profile
              </Link>
              <button onClick={handleLogout} className="hover:text-red-400">
                Logout
              </button>
            </>
          ) : (
            <>
              <Link href="/login" className="hover:text-red-400">
                Login
              </Link>
              <Link href="/register" className="hover:text-red-400">
                Register
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}