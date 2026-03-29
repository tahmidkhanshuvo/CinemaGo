'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface DashboardData {
  totalMovies: number;
  totalHalls: number;
  totalUsers: number;
  todayOrders: number;
  todayTicketRevenue: number;
  monthTicketRevenue: number;
  monthOttRevenue: number;
  monthTotalRevenue: number;
  recentOrders: Array<{
    id: number;
    bookingCode: string;
    userName: string;
    movieTitle: string;
    amount: number;
    status: string;
    createdAt: string;
  }>;
}

export default function AdminPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [dashboard, setDashboard] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
      return;
    }

    const fetchDashboard = async () => {
      try {
        const response = await api.getAdminDashboard() as any;
        if (response.success && response.data) {
          setDashboard(response.data);
        }
      } catch (error) {
        console.error('Error fetching dashboard:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, [isAuthenticated, user, router]);

  if (!isAuthenticated || user?.role !== 'Admin') return null;

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-white">Dashboard Overview</h2>
        <p className="text-gray-400 text-sm">Welcome back, {user?.fullName}</p>
      </div>

      {loading ? (
        <div className="text-white">Loading...</div>
      ) : dashboard && (
        <>
          {/* Quick Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-gray-800 rounded-lg p-4">
              <div className="text-gray-400 text-sm">Total Movies</div>
              <div className="text-2xl font-bold text-white">{dashboard.totalMovies}</div>
            </div>
            <div className="bg-gray-800 rounded-lg p-4">
              <div className="text-gray-400 text-sm">Cinema Halls</div>
              <div className="text-2xl font-bold text-white">{dashboard.totalHalls}</div>
            </div>
            <div className="bg-gray-800 rounded-lg p-4">
              <div className="text-gray-400 text-sm">Total Users</div>
              <div className="text-2xl font-bold text-white">{dashboard.totalUsers}</div>
            </div>
            <div className="bg-gray-800 rounded-lg p-4">
              <div className="text-gray-400 text-sm">Today's Ticket Revenue</div>
              <div className="text-2xl font-bold text-green-400">${(dashboard.todayTicketRevenue ?? 0).toFixed(2)}</div>
            </div>
            <div className="bg-gray-800 rounded-lg p-4">
              <div className="text-gray-400 text-sm">Today's OTT Revenue</div>
              <div className="text-2xl font-bold text-blue-400">${(dashboard.monthOttRevenue ?? 0).toFixed(2)}</div>
            </div>
          </div>

          {/* Monthly Revenue */}
          <div className="bg-gradient-to-r from-red-600 to-red-800 rounded-lg p-6 mb-6">
            <div className="text-white/80 text-sm">Monthly Revenue</div>
            <div className="text-4xl font-bold text-white">${(dashboard.monthTotalRevenue ?? 0).toFixed(2)}</div>
          </div>

          {/* Recent Orders */}
          <div className="bg-gray-800 rounded-lg p-6">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-xl font-bold text-white">Recent Orders</h3>
              <Link href="/admin/bookings" className="text-red-400 hover:text-red-300 text-sm">
                View All →
              </Link>
            </div>
            {dashboard.recentOrders.length === 0 ? (
              <p className="text-gray-400">No recent orders</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="text-left text-gray-400 border-b border-gray-700">
                      <th className="pb-3">Code</th>
                      <th className="pb-3">User</th>
                      <th className="pb-3">Movie</th>
                      <th className="pb-3">Amount</th>
                      <th className="pb-3">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.recentOrders.map((order) => (
                      <tr key={order.id} className="border-b border-gray-700/50">
                        <td className="py-3 text-white font-mono text-sm">{order.bookingCode}</td>
                        <td className="py-3 text-gray-400">{order.userName}</td>
                        <td className="py-3 text-gray-400">{order.movieTitle}</td>
                        <td className="py-3 text-white">${order.amount}</td>
                        <td className="py-3">
                          <span className={`px-2 py-1 rounded text-xs ${
                            order.status === 'Paid' ? 'bg-green-600' : 'bg-gray-600'
                          } text-white`}>
                            {order.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}