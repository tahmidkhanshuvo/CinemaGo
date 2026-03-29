'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface ShowTime {
  id: number;
  movieId: number;
  movieTitle: string;
  hallId: number;
  hallName: string;
  showDate: string;
  startTime: string;
  endTime: string;
  basePrice: number;
  status: string;
  bookedSeats: number;
}

interface Movie {
  id: number;
  title: string;
}

interface Hall {
  id: number;
  name: string;
}

export default function AdminShowTimesPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [showtimes, setShowtimes] = useState<ShowTime[]>([]);
  const [movies, setMovies] = useState<Movie[]>([]);
  const [halls, setHalls] = useState<Hall[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingShowTime, setEditingShowTime] = useState<ShowTime | null>(null);
  const [formData, setFormData] = useState({
    movieId: 0,
    hallId: 0,
    showDate: '',
    startTime: '',
    basePrice: 500,
    durationMinutes: 120
  });

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
      return;
    }

    const fetchData = async () => {
      try {
        const showtimesRes = await api.getAdminShowTimes() as any;
        const moviesRes = await api.getAdminMovies({ movieType: 'CINEMA' }) as any;
        const hallsRes = await api.getAdminHalls() as any;
        
        if (showtimesRes.success) setShowtimes(showtimesRes.data || []);
        if (moviesRes.success) setMovies(moviesRes.data || []);
        if (hallsRes.success) setHalls(hallsRes.data || []);
      } catch (error) {
        console.error('Error fetching data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [isAuthenticated, user, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingShowTime) {
        const response = await api.updateShowTime(editingShowTime.id, formData) as any;
        if (response.success) {
          setShowtimes(showtimes.map(st => st.id === editingShowTime.id ? response.data : st));
          resetForm();
        }
      } else {
        const response = await api.createShowTime(formData) as any;
        if (response.success) {
          setShowtimes([...showtimes, response.data]);
          resetForm();
        }
      }
    } catch (error) {
      console.error('Error saving showtime:', error);
    }
  };

  const handleEdit = (st: ShowTime) => {
    setEditingShowTime(st);
    setFormData({
      movieId: st.movieId,
      hallId: st.hallId,
      showDate: st.showDate.split('T')[0],
      startTime: st.startTime,
      basePrice: st.basePrice,
      durationMinutes: 120
    });
    setShowForm(true);
  };

  const resetForm = () => {
    setShowForm(false);
    setEditingShowTime(null);
    setFormData({ movieId: 0, hallId: 0, showDate: '', startTime: '', basePrice: 500, durationMinutes: 120 });
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this showtime?')) return;
    try {
      const response = await api.deleteShowTime(id) as any;
      if (response.success) {
        setShowtimes(showtimes.filter(st => st.id !== id));
      }
    } catch (error) {
      console.error('Error deleting showtime:', error);
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString();
  };

  if (!isAuthenticated || user?.role !== 'Admin') return null;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold text-white">Showtimes</h2>
          <p className="text-gray-400 text-sm">Manage movie show schedules</p>
        </div>
        <button
          onClick={() => { resetForm(); setShowForm(!showForm); }}
          className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
        >
          {showForm ? 'Cancel' : '+ Add Showtime'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="bg-gray-800 p-6 rounded-lg mb-6">
          <h3 className="text-white font-semibold mb-4">{editingShowTime ? 'Edit Showtime' : 'Add New Showtime'}</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <select
              value={formData.movieId}
              onChange={(e) => setFormData({...formData, movieId: parseInt(e.target.value)})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              required
            >
              <option value={0}>Select Movie</option>
              {movies.map(m => <option key={m.id} value={m.id}>{m.title}</option>)}
            </select>
            <select
              value={formData.hallId}
              onChange={(e) => setFormData({...formData, hallId: parseInt(e.target.value)})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              required
            >
              <option value={0}>Select Hall</option>
              {halls.map(h => <option key={h.id} value={h.id}>{h.name}</option>)}
            </select>
            <input
              type="date"
              value={formData.showDate}
              onChange={(e) => setFormData({...formData, showDate: e.target.value})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              required
            />
            <input
              type="time"
              value={formData.startTime}
              onChange={(e) => setFormData({...formData, startTime: e.target.value})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              required
            />
            <input
              type="number"
              placeholder="Base Price"
              value={formData.basePrice}
              onChange={(e) => setFormData({...formData, basePrice: parseInt(e.target.value)})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              min="100"
              required
            />
            <input
              type="number"
              placeholder="Duration (minutes)"
              value={formData.durationMinutes}
              onChange={(e) => setFormData({...formData, durationMinutes: parseInt(e.target.value)})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              min="30"
              required
            />
          </div>
          <div className="flex gap-4 mt-4">
            <button
              type="submit"
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-500"
            >
              {editingShowTime ? 'Update Showtime' : 'Create Showtime'}
            </button>
            {editingShowTime && (
              <button
                type="button"
                onClick={resetForm}
                className="px-6 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-500"
              >
                Cancel
              </button>
            )}
          </div>
        </form>
      )}

      {loading ? (
        <div className="text-white">Loading...</div>
      ) : showtimes.length === 0 ? (
        <div className="bg-gray-800 rounded-lg p-8 text-center">
          <p className="text-gray-400 mb-4">No showtimes scheduled</p>
          <button onClick={() => setShowForm(true)} className="text-red-500 hover:text-red-400">
            Create your first showtime
          </button>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="text-left text-gray-400 border-b border-gray-700">
                <th className="pb-3">Movie</th>
                <th className="pb-3">Hall</th>
                <th className="pb-3">Date</th>
                <th className="pb-3">Time</th>
                <th className="pb-3">Price</th>
                <th className="pb-3">Status</th>
                <th className="pb-3">Seats</th>
                <th className="pb-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {showtimes.map((st) => (
                <tr key={st.id} className="border-b border-gray-700/50">
                  <td className="py-3 text-white">{st.movieTitle}</td>
                  <td className="py-3 text-gray-400">{st.hallName}</td>
                  <td className="py-3 text-gray-400">{formatDate(st.showDate)}</td>
                  <td className="py-3 text-gray-400">{st.startTime}</td>
                  <td className="py-3 text-white">${st.basePrice}</td>
                  <td className="py-3">
                    <span className={`px-2 py-1 rounded text-xs ${
                      st.status === 'Running' ? 'bg-green-600' : 
                      st.status === 'Scheduled' ? 'bg-blue-600' : 'bg-gray-600'
                    } text-white`}>
                      {st.status}
                    </span>
                  </td>
                  <td className="py-3 text-gray-400">{st.bookedSeats}</td>
                  <td className="py-3">
                    <button 
                      onClick={() => handleEdit(st)}
                      className="text-blue-400 hover:text-blue-300 text-sm mr-3"
                    >
                      Edit
                    </button>
                    <button 
                      onClick={() => handleDelete(st.id)}
                      className="text-red-400 hover:text-red-300 text-sm"
                    >
                      Delete
                    </button>
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