'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Movie {
  id: number;
  title: string;
  slug: string;
  posterUrl: string;
  movieType: string;
  isPublished: boolean;
  isFeatured: boolean;
  ottPrice?: number;
}

export default function AdminMoviesPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
      return;
    }

    const fetchMovies = async () => {
      try {
        const response = await api.getAdminMovies() as any;
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
  }, [isAuthenticated, user, router]);

  if (!isAuthenticated || user?.role !== 'Admin') return null;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold text-white">All Movies</h2>
          <p className="text-gray-400 text-sm">Manage your movie library</p>
        </div>
        <Link 
          href="/admin/movies/new"
          className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 flex items-center gap-2"
        >
          <span>+</span> Add Movie
        </Link>
      </div>

      {loading ? (
        <div className="text-white">Loading...</div>
      ) : movies.length === 0 ? (
        <div className="bg-gray-800 rounded-lg p-8 text-center">
          <p className="text-gray-400 mb-4">No movies found</p>
          <Link 
            href="/admin/movies/new"
            className="text-red-500 hover:text-red-400"
          >
            Add your first movie
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {movies.map((movie) => (
            <div key={movie.id} className="bg-gray-800 rounded-lg overflow-hidden hover:ring-2 hover:ring-red-500 transition-all">
              {movie.posterUrl && (
                <div className="relative h-48 bg-gray-700">
                  <img 
                    src={movie.posterUrl} 
                    alt={movie.title}
                    className="w-full h-full object-cover"
                  />
                  <div className="absolute top-2 right-2">
                    <span className={`text-xs px-2 py-1 rounded ${
                      movie.movieType === 'CINEMA' ? 'bg-blue-600' : 
                      movie.movieType === 'OTT' ? 'bg-purple-600' : 'bg-green-600'
                    } text-white`}>
                      {movie.movieType}
                    </span>
                  </div>
                </div>
              )}
              <div className="p-4">
                <h3 className="text-white font-semibold truncate">{movie.title}</h3>
                {movie.ottPrice && (
                  <p className="text-gray-400 text-sm">Price: ${movie.ottPrice}</p>
                )}
                <div className="mt-3 flex gap-2">
                  <span className={`text-xs px-2 py-1 rounded ${movie.isPublished ? 'bg-green-600' : 'bg-red-600'} text-white`}>
                    {movie.isPublished ? 'Published' : 'Draft'}
                  </span>
                  {movie.isFeatured && (
                    <span className="text-xs px-2 py-1 rounded bg-yellow-600 text-white">
                      Featured
                    </span>
                  )}
                </div>
                <div className="mt-3 flex gap-2">
                  <Link 
                    href={`/admin/movies/edit/${movie.id}`}
                    className="text-xs text-blue-400 hover:text-blue-300"
                  >
                    Edit
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}