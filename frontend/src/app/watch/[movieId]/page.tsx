'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Navbar from '@/components/layout/Navbar';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Movie {
  id: number;
  title: string;
  description: string;
  videoUrl: string;
  posterUrl: string;
}

export default function WatchMoviePage() {
  const params = useParams();
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [movie, setMovie] = useState<Movie | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }

    const fetchMovie = async () => {
      try {
        const response = await api.watchMovie(Number(params.movieId)) as any;
        if (response.success && response.data) {
          setMovie(response.data);
        } else {
          setError('Movie not found');
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load movie');
      } finally {
        setLoading(false);
      }
    };

    fetchMovie();
  }, [params.movieId, isAuthenticated, router]);

  if (!isAuthenticated) return null;

  if (loading) return (
    <div className="min-h-screen bg-black">
      <Navbar />
      <div className="flex items-center justify-center h-[80vh]">
        <div className="text-white text-xl">Loading...</div>
      </div>
    </div>
  );

  if (error) return (
    <div className="min-h-screen bg-black">
      <Navbar />
      <div className="flex items-center justify-center h-[80vh]">
        <div className="text-red-500 text-xl">{error}</div>
      </div>
    </div>
  );

  if (!movie) return (
    <div className="min-h-screen bg-black">
      <Navbar />
      <div className="flex items-center justify-center h-[80vh]">
        <div className="text-white text-xl">Movie not found</div>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen bg-black">
      <Navbar />

      {/* Header */}
      <div className="bg-gray-900 p-4">
        <button
          onClick={() => router.back()}
          className="text-white hover:text-red-500"
        >
          ← Back
        </button>
      </div>

      {/* Video Player */}
      <div className="w-full aspect-video bg-black">
        {movie.videoUrl ? (
          <video
            controls
            autoPlay
            className="w-full h-full"
            src={movie.videoUrl}
          >
            Your browser does not support the video tag.
          </video>
        ) : (
          <div className="flex items-center justify-center h-full text-gray-500">
            No video available
          </div>
        )}
      </div>

      {/* Movie Info */}
      <div className="p-6 max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-white mb-2">{movie.title}</h1>
        <p className="text-gray-400">{movie.description}</p>
      </div>
    </div>
  );
}
