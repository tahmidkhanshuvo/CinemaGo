'use client';

import { useEffect, useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import Navbar from '@/components/layout/Navbar';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Seat {
  id: number;
  seatLabel: string;
  rowLabel: string;
  categoryName: string;
  price: number;
  isAvailable: boolean;
}

interface ShowTimeData {
  id: number;
  movieTitle: string;
  moviePosterUrl: string;
  hallName: string;
  showDate: string;
  startTime: string;
  basePrice: number;
  seats: Seat[];
}

export default function BookingPage() {
  const { showtimeId } = useParams();
  const router = useRouter();
  const { isAuthenticated } = useAuthStore();
  const [showTime, setShowTime] = useState<ShowTimeData | null>(null);
  const [selectedSeats, setSelectedSeats] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [booking, setBooking] = useState(false);

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }

    const fetchShowTime = async () => {
      try {
        const response = await api.getShowTimeWithSeats(Number(showtimeId)) as any;
        if (response.success && response.data) {
          setShowTime(response.data);
        }
      } catch (error) {
        console.error('Error fetching showtime:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchShowTime();
  }, [showtimeId, isAuthenticated, router]);

  const toggleSeat = (seatId: number) => {
    if (selectedSeats.includes(seatId)) {
      setSelectedSeats(selectedSeats.filter(id => id !== seatId));
    } else {
      setSelectedSeats([...selectedSeats, seatId]);
    }
  };

  const getTotalPrice = () => {
    if (!showTime) return 0;
    return showTime.seats
      .filter(s => selectedSeats.includes(s.id))
      .reduce((sum, s) => sum + s.price, 0);
  };

  const handleBooking = async () => {
    if (selectedSeats.length === 0) return;

    setBooking(true);
    try {
      const response = await api.bookTickets({
        showTimeId: Number(showtimeId),
        seatIds: selectedSeats,
        paymentMethod: 'DummyCard'
      }) as any;

      if (response.success) {
        alert('Booking successful! Your booking code: ' + response.data.bookingCode);
        router.push('/profile');
      } else {
        alert('Booking failed: ' + response.message);
      }
    } catch (error) {
      console.error('Error booking:', error);
      alert('Booking failed');
    } finally {
      setBooking(false);
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' });
  };

  const formatTime = (timeStr: string) => {
    const [hours, minutes] = timeStr.split(':');
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const hour12 = hour % 12 || 12;
    return `${hour12}:${minutes} ${ampm}`;
  };

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

  if (!showTime) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navbar />
        <div className="flex justify-center items-center h-[80vh]">
          <div className="text-white text-2xl">Showtime not found</div>
        </div>
      </div>
    );
  }

  // Group seats by row
  const rows = [...new Set(showTime.seats.map(s => s.rowLabel))].sort();

  return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      
      <div className="container mx-auto py-8 px-4">
        {/* Movie Info */}
        <div className="bg-gray-800 rounded-lg p-6 mb-8 flex gap-6">
          <div className="w-32 h-48 bg-gray-700 rounded overflow-hidden">
            {showTime.moviePosterUrl && (
              <img 
                src={showTime.moviePosterUrl} 
                alt={showTime.movieTitle}
                className="w-full h-full object-cover"
              />
            )}
          </div>
          <div>
            <h1 className="text-3xl font-bold text-white">{showTime.movieTitle}</h1>
            <p className="text-gray-400 mt-2">{showTime.hallName}</p>
            <p className="text-gray-400">{formatDate(showTime.showDate)} at {formatTime(showTime.startTime)}</p>
          </div>
        </div>

        {/* Screen */}
        <div className="bg-gray-700 h-16 rounded-t-3xl mb-8 flex items-center justify-center">
          <span className="text-gray-400 text-sm">SCREEN</span>
        </div>

        {/* Seats */}
        <div className="flex flex-col items-center gap-4 mb-8">
          {rows.map(row => (
            <div key={row} className="flex gap-2">
              <span className="w-8 text-gray-400 text-center">{row}</span>
              {showTime.seats
                .filter(s => s.rowLabel === row)
                .sort((a, b) => a.seatLabel.localeCompare(b.seatLabel))
                .map(seat => (
                  <button
                    key={seat.id}
                    onClick={() => seat.isAvailable && toggleSeat(seat.id)}
                    disabled={!seat.isAvailable}
                    className={`w-10 h-10 rounded-t-lg text-xs font-medium ${
                      !seat.isAvailable
                        ? 'bg-red-900 cursor-not-allowed'
                        : selectedSeats.includes(seat.id)
                          ? 'bg-red-600 text-white'
                          : 'bg-green-600 text-white hover:bg-green-500'
                    }`}
                  >
                    {seat.seatLabel.replace(row, '')}
                  </button>
                ))}
              <span className="w-8 text-gray-400 text-center">{row}</span>
            </div>
          ))}
        </div>

        {/* Legend */}
        <div className="flex justify-center gap-8 mb-8">
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 bg-green-600 rounded"></div>
            <span className="text-gray-400">Available</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 bg-red-600 rounded"></div>
            <span className="text-gray-400">Selected</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 bg-red-900 rounded"></div>
            <span className="text-gray-400">Occupied</span>
          </div>
        </div>

        {/* Booking Summary */}
        <div className="bg-gray-800 rounded-lg p-6">
          <h3 className="text-white font-semibold mb-4">Booking Summary</h3>
          <div className="flex justify-between items-center mb-4">
            <span className="text-gray-400">Selected Seats:</span>
            <span className="text-white">
              {selectedSeats.length > 0 
                ? showTime.seats.filter(s => selectedSeats.includes(s.id)).map(s => s.seatLabel).join(', ')
                : 'None'}
            </span>
          </div>
          <div className="flex justify-between items-center text-xl">
            <span className="text-white">Total:</span>
            <span className="text-red-400 font-bold">${getTotalPrice()}</span>
          </div>
          <button
            onClick={handleBooking}
            disabled={selectedSeats.length === 0 || booking}
            className="w-full mt-6 py-3 bg-red-600 hover:bg-red-700 text-white font-semibold rounded disabled:opacity-50"
          >
            {booking ? 'Processing...' : 'Confirm Booking'}
          </button>
        </div>
      </div>
    </div>
  );
}