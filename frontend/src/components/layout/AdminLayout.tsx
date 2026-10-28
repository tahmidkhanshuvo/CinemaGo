'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/auth';

interface AdminLayoutProps {
  children: React.ReactNode;
}

const menuItems = [
  { name: 'Dashboard', href: '/admin', icon: '📊' },
  { name: 'Movies', href: '/admin/movies', icon: '🎬' },
  { name: 'Halls', href: '/admin/halls', icon: '🏠' },
  { name: 'Showtimes', href: '/admin/showtimes', icon: '🕐' },
  { name: 'Bookings', href: '/admin/bookings', icon: '🎟️' },
];

export default function AdminLayout({ children }: AdminLayoutProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, isAuthenticated, logout } = useAuthStore();
  const [isCollapsed, setIsCollapsed] = useState(false);

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
    }
  }, [isAuthenticated, user, router]);

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  if (!isAuthenticated || user?.role !== 'Admin') {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-900 flex">
      {/* Sidebar */}
      <aside 
        className={`fixed left-0 top-0 h-screen flex flex-col bg-gray-800 border-r border-gray-700 transition-all duration-300 z-50 ${
          isCollapsed ? 'w-16' : 'w-64'
        }`}
      >
        {/* Logo/Header */}
        <div className="p-4 border-b border-gray-700">
          <div className="flex items-center justify-between">
            {!isCollapsed && (
              <Link href="/admin" className="text-2xl font-bold text-red-500">
                CinemaGo
              </Link>
            )}
            <button
              onClick={() => setIsCollapsed(!isCollapsed)}
              className="p-2 rounded hover:bg-gray-700 text-gray-400"
            >
              {isCollapsed ? '→' : '←'}
            </button>
          </div>
          {!isCollapsed && (
            <p className="text-xs text-gray-500 mt-1">Admin Panel</p>
          )}
        </div>

        {/* Navigation Menu */}
        <nav className="p-2 flex-1 overflow-y-auto">
          {menuItems.map((item) => {
            const isActive = pathname === item.href || 
              (item.href.includes('?') && pathname.includes(item.href.split('?')[0]));
            
            return (
              <Link
                key={item.name}
                href={item.href}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-lg mb-1 transition-colors ${
                  isActive 
                    ? 'bg-red-600 text-white' 
                    : 'text-gray-400 hover:bg-gray-700 hover:text-white'
                }`}
                title={isCollapsed ? item.name : undefined}
              >
                <span className="text-lg">{item.icon}</span>
                {!isCollapsed && <span className="font-medium">{item.name}</span>}
              </Link>
            );
          })}
        </nav>

        {/* Bottom Actions */}
        <div className="mt-auto p-2 border-t border-gray-700">
          <Link href="/profile" className="mb-1 flex items-center gap-3 rounded-lg px-3 py-2.5 text-gray-400 hover:bg-gray-700 hover:text-white" title="Account security">
            <span aria-hidden="true">🔒</span>
            {!isCollapsed && <span>Account security</span>}
          </Link>
          <Link
            href="/"
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-gray-400 hover:bg-gray-700 hover:text-white mb-1"
            title={isCollapsed ? 'View Site' : undefined}
          >
            <span className="text-lg">🌐</span>
            {!isCollapsed && <span>View Site</span>}
          </Link>
          <button
            onClick={handleLogout}
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-gray-400 hover:bg-gray-700 hover:text-white w-full"
            title={isCollapsed ? 'Logout' : undefined}
          >
            <span className="text-lg">🚪</span>
            {!isCollapsed && <span>Logout</span>}
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className={`flex-1 overflow-auto transition-all duration-300 ${isCollapsed ? 'ml-16' : 'ml-64'}`}>
        {/* Top Header */}
        <header className="bg-gray-800 border-b border-gray-700 p-4 flex justify-between items-center">
          <div>
            <h1 className="text-xl font-semibold text-white">
              {menuItems.find(item => item.href === pathname)?.name || 'Admin'}
            </h1>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="text-white font-medium">{user?.fullName}</p>
              <p className="text-gray-400 text-sm">{user?.email}</p>
            </div>
            <div className="w-10 h-10 bg-red-600 rounded-full flex items-center justify-center text-white font-bold">
              {user?.fullName?.charAt(0) || 'A'}
            </div>
          </div>
        </header>

        {/* Page Content */}
        <div className="p-6">
          {children}
        </div>
      </main>
    </div>
  );
}
