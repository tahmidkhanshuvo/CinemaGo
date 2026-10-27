'use client';

import Link from 'next/link';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Ticket {
  id: number;
  bookingCode: string;
  movieTitle: string;
  hallName: string;
  showDate: string;
  startTime: string;
  seats: string[];
  totalAmount: number;
  status: string;
  createdAt: string;
}

export default function TicketsPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }

    const fetchTickets = async () => {
      try {
        const response = await api.getTickets() as any;
        if (response.success && response.data) {
          setTickets(response.data.ticketOrders ?? []);
        }
      } catch (error) {
        console.error('Error fetching tickets:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchTickets();
  }, [isAuthenticated, router]);

  if (!isAuthenticated) return null;

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold text-white mb-6">My Cinema Tickets</h2>

      {loading ? (
        <div className="text-white">Loading...</div>
      ) : tickets.length === 0 ? (
        <div className="bg-gray-800 rounded-lg p-8 text-center">
          <p className="text-gray-400">No cinema tickets yet</p>
          <Link href="/cinema" className="text-red-500 hover:text-red-400 mt-2 inline-block">
            Browse cinema movies
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {tickets.map((ticket) => (
            <div key={ticket.id} className="bg-gray-800 rounded-lg p-6">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-white font-bold text-xl">{ticket.movieTitle}</h3>
                  <p className="text-gray-400">{ticket.hallName}</p>
                </div>
                <div className="text-right">
                  <span className={`px-3 py-1 rounded text-sm ${
                    ticket.status === 'Paid' ? 'bg-green-600' :
                    ticket.status === 'Pending' ? 'bg-yellow-600' : 'bg-red-600'
                  } text-white`}>
                    {ticket.status}
                  </span>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div>
                  <p className="text-gray-500">Date</p>
                  <p className="text-white">{new Date(ticket.showDate).toLocaleDateString()}</p>
                </div>
                <div>
                  <p className="text-gray-500">Time</p>
                  <p className="text-white">{ticket.startTime}</p>
                </div>
                <div>
                  <p className="text-gray-500">Seats</p>
                  <p className="text-white">{ticket.seats.join(', ')}</p>
                </div>
                <div>
                  <p className="text-gray-500">Amount</p>
                  <p className="text-white font-bold">${ticket.totalAmount}</p>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t border-gray-700">
                <p className="text-gray-500 text-sm">Booking Code: <span className="text-white font-mono">{ticket.bookingCode}</span></p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
