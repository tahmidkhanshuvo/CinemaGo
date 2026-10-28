'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Navbar from '@/components/layout/Navbar';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';
import Link from 'next/link';
import ChangePasswordForm from '@/components/account/ChangePasswordForm';

interface Purchase {
  id: number;
  movieId: number;
  movieTitle: string;
  moviePosterUrl: string;
  amount: number;
  purchasedAt: string;
}

interface Ticket {
  id: number;
  bookingCode: string;
  movieTitle: string;
  moviePosterUrl: string;
  hallName: string;
  showDate: string;
  startTime: string;
  totalAmount: number;
  status: string;
  seats: string[];
}

export default function ProfilePage() {
  const router = useRouter();
  const { user, isAuthenticated, logout, updateUser } = useAuthStore();
  const [purchases, setPurchases] = useState<Purchase[]>([]);
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'profile' | 'purchases' | 'tickets'>('profile');
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState({ fullName: '', phone: '' });
  const [saving, setSaving] = useState(false);
  const [updateError, setUpdateError] = useState('');
  const [updateSuccess, setUpdateSuccess] = useState('');

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }

    const fetchData = async () => {
      try {
        const profileRes = await api.getPurchases() as any;
        if (profileRes.success && profileRes.data) {
          if (Array.isArray(profileRes.data.ottPurchases)) {
            setPurchases(profileRes.data.ottPurchases);
          }
          if (Array.isArray(profileRes.data.ticketOrders)) {
            setTickets(profileRes.data.ticketOrders);
          }
        }
      } catch (error) {
        console.error('Error fetching data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [isAuthenticated, router]);

  useEffect(() => {
    if (user) {
      setEditForm({
        fullName: user.fullName || '',
        phone: user.phone || ''
      });
    }
  }, [user]);

  const handleLogout = () => {
    logout();
    router.push('/');
  };

  const handleSaveProfile = async () => {
    setSaving(true);
    setUpdateError('');
    setUpdateSuccess('');
    
    try {
      const res = await api.updateProfile(editForm) as any;
      if (res.success) {
        updateUser(res.data);
        setUpdateSuccess('Profile updated successfully!');
        setIsEditing(false);
        setTimeout(() => setUpdateSuccess(''), 3000);
      } else {
        setUpdateError(res.message || 'Failed to update profile');
      }
    } catch (error) {
      setUpdateError('An error occurred while updating profile');
    } finally {
      setSaving(false);
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  };

  if (!isAuthenticated) return null;

  return (
    <div className="min-h-screen bg-gray-900">
      <Navbar />
      
      <div className="container mx-auto py-8 px-4">
        {/* Tabs */}
        <div className="flex flex-wrap gap-4 mb-8">
          <button
            onClick={() => setActiveTab('profile')}
            className={`px-6 py-2 rounded-lg font-semibold ${
              activeTab === 'profile' 
                ? 'bg-red-600 text-white' 
                : 'bg-gray-800 text-gray-400 hover:text-white'
            }`}
          >
            My Profile
          </button>
          <button
            onClick={() => setActiveTab('purchases')}
            className={`px-6 py-2 rounded-lg font-semibold ${
              activeTab === 'purchases' 
                ? 'bg-red-600 text-white' 
                : 'bg-gray-800 text-gray-400 hover:text-white'
            }`}
          >
            My Purchases (OTT)
          </button>
          <button
            onClick={() => setActiveTab('tickets')}
            className={`px-6 py-2 rounded-lg font-semibold ${
              activeTab === 'tickets' 
                ? 'bg-red-600 text-white' 
                : 'bg-gray-800 text-gray-400 hover:text-white'
            }`}
          >
            My Tickets
          </button>
        </div>

        {activeTab === 'profile' ? (
          <div className="bg-gray-800 rounded-lg p-6">
            <div className="flex justify-between items-start mb-6">
              <h2 className="text-2xl font-bold text-white">Profile Information</h2>
              {!isEditing && (
                <button
                  onClick={() => setIsEditing(true)}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded"
                >
                  Edit Profile
                </button>
              )}
            </div>

            {updateSuccess && (
              <div className="mb-4 p-3 bg-green-600 text-white rounded">
                {updateSuccess}
              </div>
            )}

            {updateError && (
              <div className="mb-4 p-3 bg-red-600 text-white rounded">
                {updateError}
              </div>
            )}

            {isEditing ? (
              <div className="max-w-md">
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-2">Full Name</label>
                  <input
                    type="text"
                    value={editForm.fullName}
                    onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })}
                    className="w-full px-4 py-2 bg-gray-700 text-white rounded border border-gray-600 focus:border-red-500 focus:outline-none"
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-2">Phone</label>
                  <input
                    type="text"
                    value={editForm.phone}
                    onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                    className="w-full px-4 py-2 bg-gray-700 text-white rounded border border-gray-600 focus:border-red-500 focus:outline-none"
                    placeholder="Enter phone number"
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-2">Email</label>
                  <input
                    type="email"
                    value={user?.email || ''}
                    disabled
                    className="w-full px-4 py-2 bg-gray-900 text-gray-500 rounded border border-gray-700"
                  />
                  <p className="text-gray-500 text-xs mt-1">Email cannot be changed</p>
                </div>
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-2">Role</label>
                  <input
                    type="text"
                    value={user?.role || ''}
                    disabled
                    className="w-full px-4 py-2 bg-gray-900 text-gray-500 rounded border border-gray-700"
                  />
                </div>
                <div className="flex gap-4">
                  <button
                    onClick={handleSaveProfile}
                    disabled={saving}
                    className="px-6 py-2 bg-red-600 hover:bg-red-700 text-white rounded disabled:opacity-50"
                  >
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                  <button
                    onClick={() => {
                      setIsEditing(false);
                      setEditForm({ fullName: user?.fullName || '', phone: user?.phone || '' });
                      setUpdateError('');
                    }}
                    className="px-6 py-2 bg-gray-600 hover:bg-gray-500 text-white rounded"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div className="max-w-md">
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-1">Full Name</label>
                  <p className="text-white text-lg">{user?.fullName || 'Not set'}</p>
                </div>
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-1">Email</label>
                  <p className="text-white text-lg">{user?.email}</p>
                </div>
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-1">Phone</label>
                  <p className="text-white text-lg">{user?.phone || 'Not set'}</p>
                </div>
                <div className="mb-4">
                  <label className="block text-gray-400 text-sm mb-1">Role</label>
                  <span className="inline-block mt-1 px-3 py-1 bg-red-600 text-white text-sm rounded">
                    {user?.role}
                  </span>
                </div>
                <div className="mt-8 pt-6 border-t border-gray-700">
                  <button
                    onClick={handleLogout}
                    className="px-6 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded"
                  >
                    Logout
                  </button>
                </div>
              </div>
            )}
            <ChangePasswordForm />
          </div>
        ) : loading ? (
          <div className="text-white text-2xl">Loading...</div>
        ) : activeTab === 'purchases' ? (
          purchases.length === 0 ? (
            <div className="text-gray-400 text-center py-12">
              <p>No purchases yet</p>
              <Link href="/ott" className="text-red-400 hover:underline mt-2 inline-block">
                Browse OTT Movies
              </Link>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {purchases.map((purchase) => (
                <div key={purchase.id} className="bg-gray-800 rounded-lg overflow-hidden">
                  <div className="aspect-video bg-gray-700">
                    {purchase.moviePosterUrl && (
                      <img 
                        src={purchase.moviePosterUrl} 
                        alt={purchase.movieTitle}
                        className="w-full h-full object-cover"
                      />
                    )}
                  </div>
                  <div className="p-4">
                    <h3 className="text-white font-semibold">{purchase.movieTitle}</h3>
                    <p className="text-gray-400 text-sm mt-1">Purchased: {formatDate(purchase.purchasedAt)}</p>
                    <p className="text-red-400 font-bold mt-2">${purchase.amount}</p>
                    <Link 
                      href={`/ott/${purchase.movieId}`}
                      className="mt-3 inline-block px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded text-sm"
                    >
                      Watch Now
                    </Link>
                  </div>
                </div>
              ))}
            </div>
          )
        ) : (
          tickets.length === 0 ? (
            <div className="text-gray-400 text-center py-12">
              <p>No tickets yet</p>
              <Link href="/cinema" className="text-red-400 hover:underline mt-2 inline-block">
                Book Cinema Tickets
              </Link>
            </div>
          ) : (
            <div className="space-y-4">
              {tickets.map((ticket) => (
                <div key={ticket.id} className="bg-gray-800 rounded-lg p-6 flex gap-6">
                  <div className="w-24 h-36 bg-gray-700 rounded overflow-hidden">
                    {ticket.moviePosterUrl && (
                      <img 
                        src={ticket.moviePosterUrl} 
                        alt={ticket.movieTitle}
                        className="w-full h-full object-cover"
                      />
                    )}
                  </div>
                  <div className="flex-1">
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="text-white font-semibold text-xl">{ticket.movieTitle}</h3>
                        <p className="text-gray-400">{ticket.hallName}</p>
                        <p className="text-gray-400">
                          {formatDate(ticket.showDate)} at {ticket.startTime}
                        </p>
                        <p className="text-gray-400 mt-1">Seats: {ticket.seats.join(', ')}</p>
                      </div>
                      <div className="text-right">
                        <span className={`inline-block px-3 py-1 rounded text-sm ${
                          ticket.status === 'Paid' ? 'bg-green-600' : 'bg-gray-600'
                        } text-white`}>
                          {ticket.status}
                        </span>
                        <p className="text-red-400 font-bold text-xl mt-2">${ticket.totalAmount}</p>
                        <p className="text-gray-400 text-sm">Code: {ticket.bookingCode}</p>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )
        )}
      </div>
    </div>
  );
}
