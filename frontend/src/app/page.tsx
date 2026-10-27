'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import Navbar from '@/components/layout/Navbar';
import api, { type HomeData, type Movie } from '@/lib/api';

function MovieShelf({ title, subtitle, movies, href }: { title: string; subtitle: string; movies: Movie[]; href: string }) {
  if (!movies.length) return null;
  return <section className="container mx-auto px-5 py-10">
    <div className="mb-6 flex items-end justify-between gap-4">
      <div><p className="mb-2 text-xs font-semibold uppercase tracking-[.2em] text-red-400">{subtitle}</p><h2 className="text-2xl font-semibold tracking-tight sm:text-3xl">{title}</h2></div>
      <Link href={href} className="shrink-0 text-sm text-zinc-400 hover:text-white">Explore all ↗</Link>
    </div>
    <div className="grid grid-cols-2 gap-x-4 gap-y-7 sm:grid-cols-3 lg:grid-cols-5">
      {movies.slice(0, 10).map(movie => <Link key={movie.id} href={`${href}/${movie.slug}`} className="group min-w-0">
        <div className="relative mb-3 aspect-[2/3] overflow-hidden rounded-xl border border-white/10 bg-zinc-800">
          {movie.posterUrl ? <img loading="lazy" src={movie.posterUrl} alt={movie.title} className="h-full w-full object-cover transition duration-300 group-hover:scale-105" /> : <div className="grid h-full place-items-center p-4 text-center text-zinc-500">{movie.title}</div>}
          <span className="absolute bottom-3 left-3 rounded-md bg-black/75 px-2 py-1 text-[10px] font-semibold uppercase tracking-wider">{href === '/cinema' ? 'In cinemas' : 'Watch at home'}</span>
        </div>
        <h3 className="truncate font-medium group-hover:text-red-400">{movie.title}</h3><p className="mt-1 truncate text-xs text-zinc-500">{movie.genre || (href === '/cinema' ? 'Find your showtime' : 'Your next movie night')}</p>
      </Link>)}
    </div>
  </section>;
}

export default function Home() {
  const [data, setData] = useState<HomeData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [slide, setSlide] = useState(0);
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    let active = true;
    setLoading(true); setError('');
    api.getHomeData().then(res => { if (active && res.data) setData(res.data); }).catch(err => { if (active) setError(err.message || 'Unable to load the movie catalogue.'); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [attempt]);
  const heroes = data?.featured?.length ? data.featured : data?.cinemaMovies?.length ? data.cinemaMovies : data?.ottMovies || [];
  const current = heroes[slide % (heroes.length || 1)];
  useEffect(() => {
    if (heroes.length < 2 || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const timer = setInterval(() => setSlide(value => value + 1), 7000);
    return () => clearInterval(timer);
  }, [heroes.length]);
  return <div className="min-h-screen bg-[#090b10] text-white">
    <Navbar />
    <main>
      <section className="relative isolate min-h-[590px] overflow-hidden border-b border-white/10">
        {current && <img key={current.id} src={current.bannerUrl || current.posterUrl} alt="" className="absolute inset-0 -z-20 h-full w-full object-cover object-top opacity-55" />}
        <div className="absolute inset-0 -z-10 bg-gradient-to-r from-[#090b10] via-[#090b10]/75 to-transparent" />
        <div className="absolute inset-0 -z-10 bg-gradient-to-t from-[#090b10] to-transparent" />
        <div className="container mx-auto px-5 pb-20 pt-24 sm:pt-32">
          <p className="mb-6 flex items-center gap-3 text-xs font-semibold uppercase tracking-[.25em] text-red-300"><span className="h-px w-8 bg-red-400" />A seat. A screen. A great story.</p>
          <h1 className="max-w-3xl text-5xl font-semibold leading-[1.08] tracking-tight sm:text-7xl">{current?.title || 'Your next great movie starts here.'}</h1>
          <p className="mt-6 max-w-xl text-base leading-relaxed text-zinc-300">{current?.description?.slice(0, 210) || 'Big-screen moments or a night on your own sofa. Discover what to watch, choose your seats, and make it a movie night.'}</p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link href={current ? `${current.movieType === 'OTT' ? '/ott' : '/cinema'}/${current.slug}` : '/cinema'} className="rounded-full bg-red-500 px-7 py-3.5 text-sm font-semibold hover:bg-red-400">{current?.movieType === 'OTT' ? 'Watch movie' : 'Find tickets'} ↗</Link>
            <Link href="/ott" className="rounded-full border border-white/25 bg-white/5 px-7 py-3.5 text-sm font-semibold hover:bg-white/10">Explore streaming</Link>
          </div>
          {heroes.length > 1 && <div className="mt-12 flex gap-2" aria-label="Featured movies">{heroes.map((movie, index) => <button key={movie.id} onClick={() => setSlide(index)} aria-label={`Show ${movie.title}`} aria-pressed={slide % heroes.length === index} className={`h-1.5 rounded-full ${slide % heroes.length === index ? 'w-10 bg-red-400' : 'w-5 bg-white/30'}`} />)}</div>}
        </div>
      </section>
      <div className="container mx-auto grid gap-4 border-b border-white/10 px-5 py-6 text-sm text-zinc-400 sm:grid-cols-3"><p>01 <span className="ml-3 text-zinc-200">Discover something worth watching</span></p><p>02 <span className="ml-3 text-zinc-200">Pick your show. Choose your seat.</span></p><p>03 <span className="ml-3 text-zinc-200">Your tickets and library, together</span></p></div>
      {loading && <div role="status" className="container mx-auto px-5 py-14"><p className="mb-6 text-zinc-400">Loading the catalogue… The server may take a moment to wake up.</p><div className="grid grid-cols-2 gap-4 sm:grid-cols-5">{Array.from({length:5}, (_,i) => <div key={i} className="aspect-[2/3] animate-pulse rounded-xl bg-white/5" />)}</div></div>}
      {error && <div role="alert" className="container mx-auto px-5 py-12"><h2 className="text-xl font-semibold">The catalogue is taking a break</h2><p className="my-3 text-zinc-400">{error}</p><button onClick={() => setAttempt(value => value + 1)} className="rounded-full border border-white/20 px-6 py-2">Try again</button></div>}
      {!loading && !error && data && <><MovieShelf title="Made for the big screen" subtitle="Now showing" movies={data.nowShowing || []} href="/cinema" /><MovieShelf title="Good stories. Anywhere." subtitle="Stream on CinemaGo" movies={data.ottMovies || []} href="/ott" /><MovieShelf title="On the horizon" subtitle="Coming soon" movies={data.comingSoon || []} href="/cinema" /></>}
      {!loading && !error && !heroes.length && <p className="container mx-auto px-5 py-16 text-zinc-400">New titles are on the way. Check back soon.</p>}
    </main>
    <footer className="mt-10 border-t border-white/10"><div className="container mx-auto flex flex-wrap justify-between gap-4 px-5 py-8 text-sm text-zinc-500"><p>CinemaGo · Every night has a story.</p><p>Student project · Demo payments only</p></div></footer>
  </div>;
}
