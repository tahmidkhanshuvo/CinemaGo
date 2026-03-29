'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Image from 'next/image';
import Link from 'next/link';
import Navbar from '@/components/layout/Navbar';

interface Movie {
  id: number;
  title: string;
  slug: string;
  description: string;
  durationMinutes: number;
  releaseDate: string;
  language: string;
  country: string;
  genre: string;
  ageRating: string;
  imDbLikeScore: number | null;
  posterUrl: string;
  bannerUrl: string;
  trailerUrl: string;
  videoUrl: string;
  movieType: string;
  ottPrice: number;
  isPublished: boolean;
  isFeatured: boolean;
  userRating: number | null;
  ratingCount: number;
}

export default function MovieDetailPage() {
  const params = useParams();
  const router = useRouter();
  const slug = params.slug as string;
  const [movie, setMovie] = useState<Movie | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [user, setUser] = useState<any>(null);
  const [rating, setRating] = useState(0);
  const [purchased, setPurchased] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const token = localStorage.getItem('token');
        const userStr = localStorage.getItem('user');
        if (userStr) {
          setUser(JSON.parse(userStr));
        }

        const headers: any = {};
        if (token) headers['Authorization'] = `Bearer ${token}`;

        const baseUrl = process.env.NEXT_PUBLIC_API_URL || '';
        const res = await fetch(`${baseUrl}/movies/${slug}`, {
          headers
        });
        const data = await res.json();

        if (data.success) {
          setMovie(data.data);
          if (data.data.userRating) {
            setRating(data.data.userRating);
          }
          if (data.data.isPurchased) {
            setPurchased(true);
          }
        } else {
          setError('Movie not found');
        }
      } catch (err) {
        setError('Failed to load movie');
      } finally {
        setLoading(false);
      }
    };

    if (slug) fetchData();
  }, [slug]);

  const handlePurchase = async () => {
    const token = localStorage.getItem('token');
    if (!token) {
      router.push('/login');
      return;
    }

    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/ott/${movie?.id}/purchase`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        }
      });
      const data = await res.json();
      if (data.success) {
        setPurchased(true);
        alert('Purchase successful! You can now watch the movie.');
      } else {
        alert(data.message || 'Purchase failed');
      }
    } catch (err) {
      alert('Purchase failed');
    }
  };

  const handleRating = async (score: number) => {
    const token = localStorage.getItem('token');
    if (!token) {
      router.push('/login');
      return;
    }

    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/ratings`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ movieId: movie?.id, score })
      });
      const data = await res.json();
      if (data.success) {
        setRating(score);
      }
    } catch (err) {
      alert('Failed to submit rating');
    }
  };

  if (loading) return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      <div className="flex items-center justify-center min-h-[80vh] text-white">Loading...</div>
    </div>
  );
  
  if (error) return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      <div className="flex items-center justify-center min-h-[80vh] text-red-500">{error}</div>
    </div>
  );
  
  if (!movie) return null;

  const isOtt = movie.movieType === 'OTT' || movie.movieType === 'BOTH';

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <Navbar />
      
      {/* Hero Banner */}
      <div className="relative h-[50vh]">
        {movie.bannerUrl && (
          <Image
            src={movie.bannerUrl}
            alt={movie.title}
            fill
            className="object-cover"
            priority
          />
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/50 to-transparent" />
      </div>

      <div className="max-w-6xl mx-auto px-4 -mt-32 relative z-10">
        <div className="flex flex-col md:flex-row gap-8">
          {/* Poster */}
          <div className="w-64 flex-shrink-0">
            <div className="relative h-96 rounded-lg overflow-hidden shadow-2xl">
              {movie.posterUrl ? (
                <Image
                  src={movie.posterUrl}
                  alt={movie.title}
                  fill
                  className="object-cover"
                />
              ) : (
                <div className="w-full h-full bg-gray-700 flex items-center justify-center">
                  No Image
                </div>
              )}
            </div>
          </div>

          {/* Info */}
          <div className="flex-1">
            <h1 className="text-4xl font-bold mb-4">{movie.title}</h1>
            
            <div className="flex flex-wrap gap-4 mb-6 text-gray-300">
              <span>{movie.durationMinutes} min</span>
              <span>{movie.language}</span>
              <span>{movie.ageRating}</span>
              {movie.imDbLikeScore && (
                <span className="text-yellow-400">★ {movie.imDbLikeScore}</span>
              )}
            </div>

            <p className="text-gray-300 mb-6">{movie.description}</p>

            {/* Actions */}
            <div className="flex gap-4 mb-8">
              {isOtt && !purchased && (
                <button
                  onClick={handlePurchase}
                  className="bg-red-600 hover:bg-red-700 px-6 py-3 rounded-lg font-semibold flex items-center gap-2"
                >
                  Buy for ${movie.ottPrice}
                </button>
              )}
              {isOtt && purchased && (
                <Link
                  href={`/watch/${movie.id}`}
                  className="bg-green-600 hover:bg-green-700 px-6 py-3 rounded-lg font-semibold"
                >
                  ▶ Watch Now
                </Link>
              )}
              {movie.trailerUrl && (
                <a
                  href={movie.trailerUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="border border-white px-6 py-3 rounded-lg font-semibold hover:bg-white/10"
                >
                  Watch Trailer
                </a>
              )}
            </div>

            {/* Rating */}
            <div className="bg-gray-800 rounded-lg p-4">
              <p className="mb-2">Rate this movie:</p>
              <div className="flex gap-2">
                {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((score) => (
                  <button
                    key={score}
                    onClick={() => handleRating(score)}
                    className={`w-10 h-10 rounded-lg font-semibold ${
                      rating >= score
                        ? 'bg-yellow-500 text-black'
                        : 'bg-gray-700 hover:bg-gray-600'
                    }`}
                  >
                    {score}
                  </button>
                ))}
              </div>
              {rating > 0 && <p className="mt-2 text-green-400">You rated this {rating}/10</p>}
            </div>
          </div>
        </div>

        {/* Cinema Showtimes */}
        {(movie.movieType === 'CINEMA' || movie.movieType === 'BOTH') && (
          <div className="mt-12">
            <h2 className="text-2xl font-bold mb-6">Book Cinema Tickets</h2>
            <Link
              href={`/cinema/${movie.slug}`}
              className="inline-block bg-blue-600 hover:bg-blue-700 px-6 py-3 rounded-lg font-semibold"
            >
              View Showtimes
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}