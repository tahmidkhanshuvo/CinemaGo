'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import Navbar from '@/components/layout/Navbar';
import api from '@/lib/api';

interface Movie {
  id: number;
  title: string;
  slug: string;
  posterUrl?: string;
  genre?: string;
}

export default function CinemaPage() {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const moviesRes = await api.getMovies({ movieType: 'CINEMA' }) as any;
        if (moviesRes.success && moviesRes.data) {
          setMovies(moviesRes.data);
        }
      } catch (error) {
        console.error('Error fetching data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      
      <div className="container mx-auto py-8 px-4">
        <h1 className="text-4xl font-bold text-white mb-8">Cinema</h1>

        {loading ? (
          <div className="text-white text-2xl">Loading...</div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-6">
            {movies.map((movie) => (
              <Link key={movie.id} href={`/cinema/${movie.slug}`}>
                <div className="bg-gray-800 rounded-lg overflow-hidden hover:scale-105 transition-transform">
                  <div className="aspect-[2/3] bg-gray-700">
                    {movie.posterUrl && (
                      <img 
                        src={movie.posterUrl} 
                        alt={movie.title}
                        className="w-full h-full object-cover"
                      />
                    )}
                  </div>
                  <div className="p-4">
                    <h3 className="text-white font-semibold truncate">{movie.title}</h3>
                    <span className="text-gray-400 text-sm">{movie.genre}</span>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}