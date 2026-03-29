'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Booking {
  id: number;
  bookingCode: string;
  userName: string;
  userEmail: string;
  movieTitle: string;
  hallName: string;
  showDate: string;
  startTime: string;
  totalAmount: number;
  status: string;
  seats: string[];
  createdAt: string;
}

export default function AdminBookingsPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
      return;
    }

    const fetchBookings = async () => {
      try {
        const response = await api.getAdminBookings() as any;
        if (response.success && response.data) {
          setBookings(response.data);
        }
      } catch (error) {
        console.error('Error fetching bookings:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, [isAuthenticated, user, router]);

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString();
  };

  const filteredBookings = bookings.filter(b => {
    if (filter === 'all') return true;
    return b.status === filter;
  });

  const handleCancel = async (id: number) => {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    try {
      const response = await api.cancelBooking(id) as any;
      if (response.success) {
        setBookings(bookings.map(b => b.id === id ? { ...b, status: 'Cancelled' } : b));
      }
    } catch (error) {
      console.error('Error cancelling booking:', error);
    }
  };

  if (!isAuthenticated || user?.role !== 'Admin') return null;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold text-white">Ticket Orders</h2>
          <p className="text-gray-400 text-sm">View all cinema ticket bookings</p>
        </div>
      </div>

      {/* Filter */}
      <div className="flex gap-2 mb-6">
        <button
          onClick={() => setFilter('all')}
          className={`px-4 py-2 rounded-lg text-sm ${
            filter === 'all' ? 'bg-red-600 text-white' : 'bg-gray-700 text-gray-300'
          }`}
        >
          All
        </button>
        <button
          onClick={() => setFilter('Paid')}
          className={`px-4 py-2 rounded-lg text-sm ${
            filter === 'Paid' ? 'bg-green-600 text-white' : 'bg-gray-700 text-gray-300'
          }`}
        >
          Paid
        </button>
        <button
          onClick={() => setFilter('Pending')}
          className={`px-4 py-2 rounded-lg text-sm ${
            filter === 'Pending' ? 'bg-yellow-600 text-white' : 'bg-gray-700 text-gray-300'
          }`}
        >
          Pending
        </button>
        <button
          onClick={() => setFilter('Cancelled')}
          className={`px-4 py-2 rounded-lg text-sm ${
            filter === 'Cancelled' ? 'bg-red-600 text-white' : 'bg-gray-700 text-gray-300'
          }`}
        >
          Cancelled
        </button>
      </div>

      {loading ? (
        <div className="text-white">Loading...</div>
      ) : filteredBookings.length === 0 ? (
        <div className="bg-gray-800 rounded-lg p-8 text-center">
          <p className="text-gray-400">No bookings found</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="text-left text-gray-400 border-b border-gray-700">
                <th className="pb-3">Code</th>
                <th className="pb-3">User</th>
                <th className="pb-3">Movie</th>
                <th className="pb-3">Hall</th>
                <th className="pb-3">Date/Time</th>
                <th className="pb-3">Seats</th>
                <th className="pb-3">Amount</th>
                <th className="pb-3">Status</th>
                <th className="pb-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredBookings.map((booking) => (
                <tr key={booking.id} className="border-b border-gray-700/50">
                  <td className="py-3 text-white font-mono text-sm">{booking.bookingCode}</td>
                  <td className="py-3 text-gray-400">
                    <div>{booking.userName}</div>
                    <div className="text-xs text-gray-500">{booking.userEmail}</div>
                  </td>
                  <td className="py-3 text-white">{booking.movieTitle}</td>
                  <td className="py-3 text-gray-400">{booking.hallName}</td>
                  <td className="py-3 text-gray-400">
                    <div>{formatDate(booking.showDate)}</div>
                    <div className="text-xs">{booking.startTime}</div>
                  </td>
                  <td className="py-3 text-gray-400 text-sm">
                    {booking.seats?.join(', ') || '-'}
                  </td>
                  <td className="py-3 text-white font-semibold">${booking.totalAmount}</td>
                  <td className="py-3">
                    <span className={`px-2 py-1 rounded text-xs ${
                      booking.status === 'Paid' ? 'bg-green-600' : 
                      booking.status === 'Pending' ? 'bg-yellow-600' : 'bg-red-600'
                    } text-white`}>
                      {booking.status}
                    </span>
                  </td>
                  <td className="py-3">
                    {booking.status !== 'Cancelled' && (
                      <button 
                        onClick={() => handleCancel(booking.id)}
                        className="text-red-400 hover:text-red-300 text-sm"
                      >
                        Cancel
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}