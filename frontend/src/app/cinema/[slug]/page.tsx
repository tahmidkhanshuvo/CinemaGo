'use client';

import { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
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

interface ShowTime {
  id: number;
  movieId: number;
  hallId: number;
  hallName: string;
  branchName: string;
  showDate: string;
  startTime: string;
  endTime: string;
  basePrice: number;
  status: string;
  availableSeats: number;
}

// Group showtimes by branch
interface BranchShowtimes {
  branchName: string;
  halls: {
    hallName: string;
    showtimes: ShowTime[];
  }[];
}

export default function CinemaMoviePage() {
  const params = useParams();
  const slug = params.slug as string;
  const [movie, setMovie] = useState<Movie | null>(null);
  const [showTimes, setShowTimes] = useState<ShowTime[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedDate, setSelectedDate] = useState<string>('');
  const [selectedBranch, setSelectedBranch] = useState<string>('');

  useEffect(() => {
    const fetchData = async () => {
      if (!slug) {
        setLoading(false);
        return;
      }
      
      const baseUrl = process.env.NEXT_PUBLIC_API_URL || '';
      
      try {
        const res = await fetch(`${baseUrl}/movies/${slug}`);
        const data = await res.json();
        console.log('Movie API response:', data);

        if (data.success && data.data) {
          setMovie(data.data);
          
          // Fetch showtimes for this movie
          const showRes = await fetch(`${baseUrl}/cinema/showtimes?movieId=${data.data.id}`);
          const showData = await showRes.json();
          console.log('Showtimes API response:', showData);
          
          if (showData.success) {
            setShowTimes(showData.data);
          }
        } else {
          console.log('Movie not found or error:', data.message);
        }
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [slug]);

  if (loading) return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      <div className="flex items-center justify-center min-h-[80vh] text-white">Loading...</div>
    </div>
  );
  if (!movie) return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      <div className="flex items-center justify-center min-h-[80vh] text-white">Movie not found</div>
    </div>
  );

  // Group showtimes by branch and hall
  const groupedShowTimes: BranchShowtimes[] = showTimes.reduce((acc: BranchShowtimes[], show) => {
    const branchName = show.branchName || show.hallName;
    let branch = acc.find(b => b.branchName === branchName);
    
    if (!branch) {
      branch = { branchName, halls: [] };
      acc.push(branch);
    }
    
    let hall = branch.halls.find(h => h.hallName === show.hallName);
    if (!hall) {
      hall = { hallName: show.hallName, showtimes: [] };
      branch.halls.push(hall);
    }
    
    hall.showtimes.push(show);
    return acc;
  }, []);

  // Get unique dates from showtimes
  const uniqueDates = [...new Set(showTimes.map(s => s.showDate.split('T')[0]))].sort().slice(0, 7);

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <Navbar />
      {/* Hero */}
      <div className="relative h-[40vh]">
        {movie.bannerUrl ? (
          <Image src={movie.bannerUrl} alt={movie.title} fill className="object-cover" priority />
        ) : (
          <div className="absolute inset-0 bg-gradient-to-br from-red-900 to-gray-900" />
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/50 to-transparent" />
      </div>

      <div className="max-w-6xl mx-auto px-4 -mt-24 relative z-10 pb-12">
        {/* Movie Info */}
        <div className="flex flex-col md:flex-row gap-8 mb-8">
          <div className="w-48 flex-shrink-0">
            <div className="relative h-72 rounded-lg overflow-hidden shadow-2xl">
              {movie.posterUrl ? (
                <Image src={movie.posterUrl} alt={movie.title} fill className="object-cover" />
              ) : (
                <div className="w-full h-full bg-gray-700" />
              )}
            </div>
          </div>

          <div className="flex-1">
            <h1 className="text-4xl font-bold mb-4">{movie.title}</h1>
            <div className="flex flex-wrap gap-4 text-gray-300 mb-4">
              <span>{movie.durationMinutes} min</span>
              <span>{movie.language}</span>
              <span>{movie.ageRating}</span>
              <span>{movie.genre}</span>
              {movie.imDbLikeScore && <span className="text-yellow-400">★ {movie.imDbLikeScore}</span>}
            </div>
            <p className="text-gray-300">{movie.description}</p>
          </div>
        </div>

        {/* Showtimes Section - With Date and Branch Selection */}
        <div className="mt-8">
          <h2 className="text-2xl font-bold mb-6">Select Showtime & Book</h2>
          
          {showTimes.length === 0 ? (
            <div className="bg-gray-800 rounded-lg p-8 text-center">
              <p className="text-gray-400">No showtimes available for this movie</p>
            </div>
          ) : (
            <div className="space-y-6">
              {/* Step 1: Date Selection */}
              <div className="bg-gray-800 rounded-lg p-6">
                <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                  <span className="bg-red-600 text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">1</span>
                  Select Date
                </h3>
                <div className="flex flex-wrap gap-2">
                  {uniqueDates.map((date) => (
                    <button
                      key={date}
                      onClick={() => { setSelectedDate(date); setSelectedBranch(''); }}
                      className={`px-4 py-2 rounded-lg font-semibold transition-all ${
                        selectedDate === date
                          ? 'bg-red-600 text-white'
                          : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                      }`}
                    >
                      {new Date(date).toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })}
                    </button>
                  ))}
                </div>
              </div>

              {/* Step 2: Branch Selection (after date selected) */}
              {selectedDate && (
                <div className="bg-gray-800 rounded-lg p-6">
                  <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                    <span className="bg-red-600 text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">2</span>
                    Select Branch/Cinema
                  </h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {groupedShowTimes
                      .filter(branch => branch.halls.some(hall => 
                        hall.showtimes.some(s => s.showDate.split('T')[0] === selectedDate)
                      ))
                      .map((branch, idx) => (
                        <button
                          key={idx}
                          onClick={() => setSelectedBranch(branch.branchName)}
                          className={`p-4 rounded-lg text-left transition-all border-2 ${
                            selectedBranch === branch.branchName
                              ? 'border-red-500 bg-red-900/30'
                              : 'border-gray-600 bg-gray-700 hover:border-gray-500'
                          }`}
                        >
                          <div className="font-semibold text-lg">{branch.branchName}</div>
                          <div className="text-gray-400 text-sm">
                            {branch.halls.length} hall(s) • {
                              branch.halls.reduce((acc, hall) => 
                                acc + hall.showtimes.filter(s => s.showDate.split('T')[0] === selectedDate).length, 0
                              )} shows
                          </div>
                        </button>
                      ))}
                  </div>
                </div>
              )}

              {/* Step 3: Showtimes (after branch selected) */}
              {selectedDate && selectedBranch && (
                <div className="bg-gray-800 rounded-lg p-6">
                  <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                    <span className="bg-red-600 text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">3</span>
                    Select Showtime
                  </h3>
                  {groupedShowTimes
                    .filter(b => b.branchName === selectedBranch)
                    .map((branch, branchIdx) => (
                      <div key={branchIdx} className="space-y-4">
                        {branch.halls.map((hall, hallIdx) => {
                          const filteredShowtimes = hall.showtimes.filter(s => s.showDate.split('T')[0] === selectedDate);
                          if (filteredShowtimes.length === 0) return null;
                          
                          return (
                            <div key={hallIdx}>
                              <h4 className="text-md font-semibold text-white mb-3 flex items-center gap-2">
                                <span>🎬</span> {hall.hallName}
                              </h4>
                              <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-3">
                                {filteredShowtimes
                                  .sort((a, b) => a.startTime.localeCompare(b.startTime))
                                  .map((show) => (
                                    <Link
                                      key={show.id}
                                      href={`/booking/${show.id}`}
                                      className="bg-gray-700 hover:bg-red-600 hover:scale-105 transition-all p-3 rounded-lg text-center border border-gray-600 hover:border-red-400"
                                    >
                                      <div className="font-semibold text-lg">{show.startTime}</div>
                                      <div className="text-green-400 text-sm font-bold">${show.basePrice}</div>
                                      <div className={`text-xs mt-1 ${show.availableSeats > 10 ? 'text-green-400' : show.availableSeats > 0 ? 'text-yellow-400' : 'text-red-400'}`}>
                                        {show.availableSeats > 0 ? `${show.availableSeats} seats` : 'Sold out'}
                                      </div>
                                      <div className="mt-2 bg-red-600 text-white text-xs py-1 px-2 rounded font-semibold">
                                        BOOK NOW
                                      </div>
                                    </Link>
                                  ))}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    ))}
                </div>
              )}

              {/* Fallback: If nothing selected, show all showtimes */}
              {!selectedDate && (
                <div className="bg-gray-800 rounded-lg p-4">
                  <p className="text-gray-400 text-center">Select a date above to browse available showtimes</p>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Movie Info Section */}
        <div className="mt-12 grid grid-cols-1 md:grid-cols-2 gap-8">
          <div className="bg-gray-800 rounded-lg p-6">
            <h3 className="text-xl font-bold mb-4">Movie Details</h3>
            <div className="space-y-2 text-gray-300">
              <p><span className="font-semibold">Duration:</span> {movie.durationMinutes} minutes</p>
              <p><span className="font-semibold">Language:</span> {movie.language}</p>
              <p><span className="font-semibold">Genre:</span> {movie.genre}</p>
              <p><span className="font-semibold">Country:</span> {movie.country}</p>
              <p><span className="font-semibold">Age Rating:</span> {movie.ageRating}</p>
              {movie.imDbLikeScore && (
                <p><span className="font-semibold">IMDb Score:</span> <span className="text-yellow-400">★ {movie.imDbLikeScore}</span></p>
              )}
            </div>
          </div>
          
          {movie.trailerUrl && (
            <div className="bg-gray-800 rounded-lg p-6">
              <h3 className="text-xl font-bold mb-4">Watch Trailer</h3>
              <a 
                href={movie.trailerUrl} 
                target="_blank" 
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 bg-red-600 hover:bg-red-700 px-4 py-2 rounded-lg"
              >
                <span>▶</span> Watch Trailer
              </a>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}