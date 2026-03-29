'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import Navbar from '@/components/layout/Navbar';
import api from '@/lib/api';

interface Movie {
  id: number;
  title: string;
  slug: string;
  posterUrl?: string;
  bannerUrl?: string;
  ottPrice: number;
  movieType: string;
  description?: string;
}

interface HomeData {
  featured: Movie[];
  ottMovies: Movie[];
  cinemaMovies: Movie[];
  nowShowing: Movie[];
  comingSoon: Movie[];
}

export default function Home() {
  const [homeData, setHomeData] = useState<HomeData | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentSlide, setCurrentSlide] = useState(0);

  useEffect(() => {
    const fetchHomeData = async () => {
      try {
        const response = await api.getHomeData();
        if (response.success) {
          setHomeData(response.data);
        }
      } catch (error) {
        console.error('Error fetching home data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchHomeData();
  }, []);

  // Auto-rotate carousel
  useEffect(() => {
    if (!homeData?.cinemaMovies?.length) return;
    
    const timer = setInterval(() => {
      setCurrentSlide((prev) => (prev + 1) % homeData.cinemaMovies.length);
    }, 5000);
    
    return () => clearInterval(timer);
  }, [homeData?.cinemaMovies?.length]);

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navbar />
        <div className="flex justify-center items-center h-[80vh]">
          <div className="text-white text-2xl">Loading...</div>
        </div>
      </div>
    );
  }

  const heroMovies = homeData?.cinemaMovies?.length > 0 
    ? homeData.cinemaMovies 
    : homeData?.ottMovies || [];

  return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      
      {/* Hero Carousel */}
      <div className="relative h-[70vh] overflow-hidden">
        {heroMovies.map((movie, index) => (
          <div
            key={movie.id}
            className={`absolute inset-0 transition-opacity duration-1000 ${
              index === currentSlide ? 'opacity-100' : 'opacity-0'
            }`}
          >
            {/* Background Image */}
            <div 
              className="absolute inset-0 bg-cover bg-center"
              style={{ 
                backgroundImage: movie.bannerUrl || movie.posterUrl 
                  ? `url(${movie.bannerUrl || movie.posterUrl})` 
                  : 'none',
                filter: 'brightness(0.3)'
              }}
            />
            <div className="absolute inset-0 bg-gradient-to-r from-gray-900 via-gray-900/70 to-transparent" />
            
            {/* Content */}
            <div className="relative container mx-auto h-full flex items-center px-4">
              <div className="max-w-2xl">
                <span className="inline-block px-3 py-1 bg-red-600 text-white text-sm font-semibold rounded mb-4">
                  {movie.movieType === 'CINEMA' ? 'NOW IN CINEMAS' : 'STREAMING'}
                </span>
                <h1 className="text-5xl md:text-6xl font-bold text-white mb-4">
                  {movie.title}
                </h1>
                <p className="text-gray-300 text-lg mb-6 line-clamp-2">
                  {movie.description || 'Experience the ultimate movie watching experience at CinemaGo'}
                </p>
                <div className="flex gap-4">
                  {movie.movieType === 'CINEMA' ? (
                    <Link 
                      href={`/cinema/${movie.slug}`}
                      className="px-8 py-3 bg-red-600 hover:bg-red-700 rounded-lg font-semibold text-lg"
                    >
                      Book Now
                    </Link>
                  ) : (
                    <Link 
                      href={`/ott/${movie.slug}`}
                      className="px-8 py-3 bg-red-600 hover:bg-red-700 rounded-lg font-semibold text-lg"
                    >
                      Watch Now
                    </Link>
                  )}
                  <Link 
                    href="/ott"
                    className="px-8 py-3 border-2 border-white hover:bg-white/10 rounded-lg font-semibold text-lg"
                  >
                    Browse All
                  </Link>
                </div>
              </div>
            </div>
          </div>
        ))}

        {/* Carousel Dots */}
        <div className="absolute bottom-8 left-1/2 transform -translate-x-1/2 flex gap-3">
          {heroMovies.map((_, index) => (
            <button
              key={index}
              onClick={() => setCurrentSlide(index)}
              className={`w-3 h-3 rounded-full transition-all ${
                index === currentSlide ? 'bg-red-600 w-8' : 'bg-white/50'
              }`}
            />
          ))}
        </div>
      </div>

      {/* Now Showing Section */}
      {homeData?.nowShowing && homeData.nowShowing.length > 0 && (
        <section className="container mx-auto py-12 px-4">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-3xl font-bold text-white">Now Showing</h2>
            <Link href="/cinema" className="text-red-400 hover:text-red-300">
              View All →
            </Link>
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-6">
            {homeData.nowShowing.slice(0, 5).map((movie) => (
              <Link key={movie.id} href={`/cinema/${movie.slug}`} className="group">
                <div className="relative aspect-[2/3] rounded-lg overflow-hidden mb-3">
                  {movie.posterUrl ? (
                    <Image
                      src={movie.posterUrl}
                      alt={movie.title}
                      fill
                      className="object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                  ) : (
                    <div className="w-full h-full bg-gray-800 flex items-center justify-center">
                      <span className="text-gray-500">No Image</span>
                    </div>
                  )}
                  <div className="absolute inset-0 bg-gradient-to-t from-black/80 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                </div>
                <h3 className="text-white font-semibold truncate">{movie.title}</h3>
                <p className="text-gray-400 text-sm">In Cinemas Now</p>
              </Link>
            ))}
          </div>
        </section>
      )}

      {/* OTT Movies Section */}
      {homeData?.ottMovies && homeData.ottMovies.length > 0 && (
        <section className="container mx-auto py-12 px-4">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-3xl font-bold text-white">Stream Online</h2>
            <Link href="/ott" className="text-red-400 hover:text-red-300">
              View All →
            </Link>
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {homeData.ottMovies.slice(0, 12).map((movie) => (
              <Link key={movie.id} href={`/ott/${movie.slug}`} className="group">
                <div className="relative aspect-[2/3] rounded-lg overflow-hidden mb-2">
                  {movie.posterUrl ? (
                    <Image
                      src={movie.posterUrl}
                      alt={movie.title}
                      fill
                      className="object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                  ) : (
                    <div className="w-full h-full bg-gray-800" />
                  )}
                  <div className="absolute top-2 right-2 bg-red-600 text-white text-xs font-bold px-2 py-1 rounded">
                    ${movie.ottPrice}
                  </div>
                </div>
                <h3 className="text-white text-sm font-medium truncate">{movie.title}</h3>
              </Link>
            ))}
          </div>
        </section>
      )}

      {/* Coming Soon Section */}
      {homeData?.comingSoon && homeData.comingSoon.length > 0 && (
        <section className="container mx-auto py-12 px-4">
          <h2 className="text-3xl font-bold text-white mb-6">Coming Soon</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-6">
            {homeData.comingSoon.map((movie) => (
              <Link key={movie.id} href={`/ott/${movie.slug}`} className="group">
                <div className="relative aspect-[2/3] rounded-lg overflow-hidden mb-3 opacity-75">
                  {movie.posterUrl ? (
                    <Image
                      src={movie.posterUrl}
                      alt={movie.title}
                      fill
                      className="object-cover"
                    />
                  ) : (
                    <div className="w-full h-full bg-gray-800" />
                  )}
                  <div className="absolute inset-0 border-2 border-yellow-500/50 rounded-lg" />
                </div>
                <h3 className="text-white font-semibold truncate">{movie.title}</h3>
                <p className="text-yellow-400 text-sm">Coming Soon</p>
              </Link>
            ))}
          </div>
        </section>
      )}

      {/* Quick Links */}
      <section className="bg-gray-800 py-16">
        <div className="container mx-auto px-4">
          <div className="grid md:grid-cols-3 gap-8 text-center">
            <div className="p-6">
              <div className="text-4xl mb-4">🎬</div>
              <h3 className="text-xl font-bold text-white mb-2">Watch Online</h3>
              <p className="text-gray-400">Stream thousands of movies anytime, anywhere</p>
              <Link href="/ott" className="inline-block mt-4 text-red-400 hover:text-red-300">
                Browse OTT →
              </Link>
            </div>
            <div className="p-6">
              <div className="text-4xl mb-4">🎟️</div>
              <h3 className="text-xl font-bold text-white mb-2">Book Tickets</h3>
              <p className="text-gray-400">Get the best seats for the latest blockbusters</p>
              <Link href="/cinema" className="inline-block mt-4 text-red-400 hover:text-red-300">
                Book Now →
              </Link>
            </div>
            <div className="p-6">
              <div className="text-4xl mb-4">👤</div>
              <h3 className="text-xl font-bold text-white mb-2">My Account</h3>
              <p className="text-gray-400">View your purchases and manage your profile</p>
              <Link href="/profile" className="inline-block mt-4 text-red-400 hover:text-red-300">
                View Profile →
              </Link>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
