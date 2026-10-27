'use client';

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import Navbar from '@/components/layout/Navbar';
import api, { type Movie } from '@/lib/api';

export default function Catalogue({ type }: { type: 'OTT' | 'CINEMA' }) {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [search, setSearch] = useState('');
  const [genre, setGenre] = useState('');
  const [sort, setSort] = useState('featured');
  const [page, setPage] = useState(1);
  useEffect(() => {
    let active = true;
    setLoading(true); setError('');
    api.getMovies({ movieType: type }).then(result => {
      if (active) setMovies(result.data || []);
    }).catch(reason => {
      if (active) setError(reason instanceof Error ? reason.message : 'Unable to load movies.');
    }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [type, attempt]);
  const genres = useMemo(() => [...new Set(movies.flatMap(movie => (movie.genre || '').split(',').map(value => value.trim()).filter(Boolean)))].sort(), [movies]);
  const filtered = useMemo(() => {
    const matches = movies.filter(movie => `${movie.title} ${movie.genre || ''}`.toLowerCase().includes(search.toLowerCase().trim()) && (!genre || (movie.genre || '').split(',').map(value => value.trim()).includes(genre)));
    if (sort === 'title') matches.sort((a,b) => a.title.localeCompare(b.title));
    if (sort === 'price') matches.sort((a,b) => a.ottPrice - b.ottPrice);
    return matches;
  }, [movies, search, genre, sort]);
  const pages = Math.max(1, Math.ceil(filtered.length / 20));
  const currentPage = Math.min(page, pages);
  const root = type === 'OTT' ? '/ott' : '/cinema';
  return <div className="min-h-screen bg-[#090b10] text-white">
    <Navbar />
    <main className="container mx-auto px-5 py-14">
      <p className="mb-4 text-xs font-semibold uppercase tracking-[.24em] text-red-400">{type === 'OTT' ? 'Good stories. Anywhere.' : 'Made for the big screen.'}</p>
      <h1 className="text-4xl font-semibold tracking-tight sm:text-6xl">{type === 'OTT' ? 'Make it a movie night.' : 'Find your next big-screen moment.'}</h1>
      <p className="mt-5 max-w-xl leading-relaxed text-zinc-400">{type === 'OTT' ? 'Explore the streaming catalogue. Your purchased titles stay together in your library.' : 'Browse the cinema catalogue, check available showtimes, and choose the seats that suit you.'}</p>
      <div className="my-10 grid gap-3 rounded-2xl border border-white/10 bg-white/[.025] p-4 sm:grid-cols-[1fr_180px_180px]">
        <label><span className="sr-only">Search movies</span><input type="search" placeholder="Search titles or genres…" value={search} onChange={event=>{setSearch(event.target.value);setPage(1);}} className="w-full rounded-xl border border-white/10 bg-[#11141c] px-4 py-3 outline-none focus:border-red-400" /></label>
        <label><span className="sr-only">Genre</span><select value={genre} onChange={event=>{setGenre(event.target.value);setPage(1);}} className="w-full rounded-xl border border-white/10 bg-[#11141c] px-4 py-3"><option value="">All genres</option>{genres.map(value=><option key={value}>{value}</option>)}</select></label>
        <label><span className="sr-only">Sort movies</span><select value={sort} onChange={event=>{setSort(event.target.value);setPage(1);}} className="w-full rounded-xl border border-white/10 bg-[#11141c] px-4 py-3"><option value="featured">Featured first</option><option value="title">Title: A–Z</option>{type === 'OTT' && <option value="price">Price: low to high</option>}</select></label>
      </div>
      {loading ? <p role="status" className="py-10 text-zinc-400">Loading the catalogue… Free hosting may take a moment to wake.</p> : error ? <div role="alert" className="rounded-2xl border border-red-400/25 p-8"><h2 className="text-xl">The catalogue couldn’t load.</h2><p className="my-4 text-zinc-400">{error}</p><button onClick={()=>setAttempt(value=>value+1)} className="rounded-full bg-red-500 px-6 py-3">Try again</button></div> : <>
        <p className="mb-6 text-sm text-zinc-500">{filtered.length} {filtered.length === 1 ? 'title' : 'titles'} to explore</p>
        <div className="grid grid-cols-2 gap-x-4 gap-y-8 sm:grid-cols-3 lg:grid-cols-5">{filtered.slice((currentPage-1)*20,currentPage*20).map(movie=><Link key={movie.id} href={`${root}/${movie.slug}`} className="group min-w-0">
          <div className="mb-3 aspect-[2/3] overflow-hidden rounded-xl border border-white/10 bg-zinc-800">{movie.posterUrl ? <img loading="lazy" src={movie.posterUrl} alt={movie.title} className="h-full w-full object-cover transition duration-300 group-hover:scale-105" /> : <div className="grid h-full place-items-center p-4 text-center text-zinc-500">{movie.title}</div>}</div>
          <h2 className="truncate font-medium group-hover:text-red-400">{movie.title}</h2><p className="mt-2 truncate text-xs text-zinc-500">{movie.genre || 'Explore the story'}</p>{type === 'OTT' && <p className="mt-2 text-sm text-red-300">${movie.ottPrice.toFixed(2)}</p>}
        </Link>)}</div>
        {!filtered.length && <div className="py-16 text-center"><h2 className="text-xl">No matches just yet.</h2><p className="my-4 text-zinc-400">Try a different title or broaden your genre filter.</p><button className="text-red-300" onClick={()=>{setSearch('');setGenre('');setPage(1);}}>Clear filters</button></div>}
        {pages > 1 && <nav aria-label="Catalogue pages" className="mt-12 flex items-center justify-center gap-6 text-sm"><button disabled={currentPage<=1} onClick={()=>setPage(currentPage-1)} className="rounded-full border border-white/20 px-5 py-2 disabled:opacity-30">Previous</button><span className="text-zinc-400">Page {currentPage} of {pages}</span><button disabled={currentPage>=pages} onClick={()=>setPage(currentPage+1)} className="rounded-full border border-white/20 px-5 py-2 disabled:opacity-30">Next</button></nav>}
      </>}
    </main>
  </div>;
}
