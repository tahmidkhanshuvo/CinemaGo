'use client';

import Link from 'next/link';
import { useAuthStore } from '@/store/auth';
import { useRouter, usePathname } from 'next/navigation';
import { useEffect, useState } from 'react';

export default function Navbar() {
  const { user, isAuthenticated, logout } = useAuthStore();
  const router = useRouter();
  const path = usePathname();
  const [mounted, setMounted] = useState(false);
  const [open, setOpen] = useState(false);
  useEffect(() => setMounted(true), []);
  const links = [{ href: '/ott', label: 'Stream movies' }, { href: '/cinema', label: 'In cinemas' }];
  return (
    <header className="sticky top-0 z-50 border-b border-white/10 bg-[#090b10]/95 backdrop-blur-xl">
      <nav aria-label="Main navigation" className="container mx-auto flex min-h-20 flex-wrap items-center justify-between gap-4 px-5">
        <Link href="/" className="flex items-center gap-2 text-2xl font-bold tracking-tight">
          <span aria-hidden="true" className="grid h-9 w-9 place-items-center rounded-xl bg-red-500 text-sm text-white">▶</span>
          Cinema<span className="text-red-400">Go</span>
        </Link>
        <button aria-label="Toggle navigation" aria-expanded={open} aria-controls="main-links" onClick={() => setOpen(!open)} className="rounded-lg border border-white/20 px-3 py-2 md:hidden">{open ? 'Close' : 'Menu'}</button>
        <div id="main-links" className={`${open ? 'flex' : 'hidden'} w-full flex-col gap-5 pb-5 text-sm font-medium md:flex md:w-auto md:flex-row md:items-center md:pb-0`}>
          {links.map(link => <Link key={link.href} href={link.href} aria-current={path.startsWith(link.href) ? 'page' : undefined} className={path.startsWith(link.href) ? 'text-red-400' : 'text-zinc-300 hover:text-white'}>{link.label}</Link>)}
          {mounted && isAuthenticated ? <>
            {user?.role === 'Admin' && <Link href="/admin">Admin</Link>}
            <Link href="/profile" className="text-zinc-300">My account</Link>
            <button onClick={() => { logout(); router.push('/'); }} className="text-left text-zinc-400">Sign out</button>
          </> : <>
            <Link href="/login" className="text-zinc-300">Sign in</Link>
            <Link href="/register" className="rounded-full bg-red-500 px-5 py-2.5 text-center text-white hover:bg-red-400">Join CinemaGo ↗</Link>
          </>}
        </div>
      </nav>
    </header>
  );
}
