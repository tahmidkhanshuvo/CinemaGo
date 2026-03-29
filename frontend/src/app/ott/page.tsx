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
  ottPrice: number;
  genre?: string;
  durationMinutes?: number;
}

export default function OttPage() {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    const fetchMovies = async () => {
      try {
        const response = await api.getMovies({ movieType: 'OTT' });
        if (response.success && response.data) {
          setMovies(response.data);
        }
      } catch (error) {
        console.error('Error fetching movies:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMovies();
  }, []);

  const filteredMovies = movies.filter(m => 
    m.title.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      
      <div className="container mx-auto py-8 px-4">
        <h1 className="text-4xl font-bold text-white mb-8">OTT Movies</h1>
        
        <div className="mb-8">
          <input
            type="text"
            placeholder="Search movies..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full max-w-md px-4 py-2 bg-gray-800 text-white rounded focus:outline-none focus:ring-2 focus:ring-red-500"
          />
        </div>
        
        {loading ? (
          <div className="text-white text-2xl">Loading...</div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-6">
            {filteredMovies.map((movie) => (
              <Link key={movie.id} href={`/ott/${movie.slug}`}>
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
                    <div className="flex justify-between items-center mt-2">
                      <span className="text-gray-400 text-sm">{movie.genre}</span>
                      <span className="text-red-400 font-bold">${movie.ottPrice}</span>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
        
        {!loading && filteredMovies.length === 0 && (
          <div className="text-gray-400 text-center py-12">
            No movies found
          </div>
        )}
      </div>
    </div>
  );
}