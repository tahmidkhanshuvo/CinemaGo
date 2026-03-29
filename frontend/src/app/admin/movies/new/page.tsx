'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

export default function AddMoviePage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [formData, setFormData] = useState({
    title: '',
    slug: '',
    description: '',
    durationMinutes: 120,
    releaseDate: '',
    language: 'English',
    country: 'USA',
    genre: 'Action',
    ageRating: 'PG-13',
    posterUrl: '',
    bannerUrl: '',
    trailerUrl: '',
    videoUrl: '',
    movieType: 'CINEMA',
    ottPrice: 0,
    isPublished: true,
    isFeatured: false
  });

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
    }
  }, [isAuthenticated, user, router]);

  const generateSlug = (title: string) => {
    return title.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    if (name === 'title') {
      setFormData({ ...formData, [name]: value, slug: generateSlug(value) });
    } else if (type === 'checkbox') {
      setFormData({ ...formData, [name]: (e.target as HTMLInputElement).checked });
    } else if (type === 'number') {
      setFormData({ ...formData, [name]: parseFloat(value) || 0 });
    } else {
      setFormData({ ...formData, [name]: value });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await api.createMovie(formData) as any;
      if (response.success) {
        router.push('/admin/movies');
      } else {
        setError(response.message || 'Failed to create movie');
      }
    } catch (err: any) {
      setError(err.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  if (!isAuthenticated || user?.role !== 'Admin') return null;

  return (
    <div>
      <div className="flex items-center gap-4 mb-6">
        <Link href="/admin/movies" className="text-gray-400 hover:text-white">
          ← Back
        </Link>
        <div>
          <h2 className="text-2xl font-bold text-white">Add New Movie</h2>
          <p className="text-gray-400 text-sm">Create a new movie entry</p>
        </div>
      </div>

      {error && (
        <div className="bg-red-500/20 text-red-400 p-3 rounded mb-4">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Info */}
        <div className="bg-gray-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Basic Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-gray-400 text-sm mb-1">Title *</label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                required
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Slug</label>
              <input
                type="text"
                name="slug"
                value={formData.slug}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              />
            </div>
            <div className="md:col-span-2">
              <label className="block text-gray-400 text-sm mb-1">Description</label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleChange}
                rows={3}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Movie Type *</label>
              <select
                name="movieType"
                value={formData.movieType}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              >
                <option value="CINEMA">Cinema</option>
                <option value="OTT">OTT</option>
                <option value="BOTH">Both</option>
              </select>
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Genre</label>
              <select
                name="genre"
                value={formData.genre}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              >
                <option>Action</option>
                <option>Comedy</option>
                <option>Drama</option>
                <option>Horror</option>
                <option>Romance</option>
                <option>Sci-Fi</option>
                <option>Thriller</option>
              </select>
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Duration (min)</label>
              <input
                type="number"
                name="durationMinutes"
                value={formData.durationMinutes}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                min="1"
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Release Date</label>
              <input
                type="date"
                name="releaseDate"
                value={formData.releaseDate}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Language</label>
              <input
                type="text"
                name="language"
                value={formData.language}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Age Rating</label>
              <select
                name="ageRating"
                value={formData.ageRating}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
              >
                <option>G</option>
                <option>PG</option>
                <option>PG-13</option>
                <option>R</option>
                <option>NC-17</option>
              </select>
            </div>
          </div>
        </div>

        {/* Pricing (for OTT) */}
        {(formData.movieType === 'OTT' || formData.movieType === 'BOTH') && (
          <div className="bg-gray-800 rounded-lg p-6">
            <h3 className="text-lg font-semibold text-white mb-4">Pricing (OTT)</h3>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Price ($)</label>
              <input
                type="number"
                name="ottPrice"
                value={formData.ottPrice}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                min="0"
                step="0.01"
              />
            </div>
          </div>
        )}

        {/* Media URLs */}
        <div className="bg-gray-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Media URLs</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-gray-400 text-sm mb-1">Poster URL</label>
              <input
                type="url"
                name="posterUrl"
                value={formData.posterUrl}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                placeholder="https://..."
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Banner URL</label>
              <input
                type="url"
                name="bannerUrl"
                value={formData.bannerUrl}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                placeholder="https://..."
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Trailer URL</label>
              <input
                type="url"
                name="trailerUrl"
                value={formData.trailerUrl}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                placeholder="https://youtube.com/..."
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-1">Video URL (for OTT)</label>
              <input
                type="url"
                name="videoUrl"
                value={formData.videoUrl}
                onChange={handleChange}
                className="w-full px-4 py-2 bg-gray-700 text-white rounded"
                placeholder="https://..."
              />
            </div>
          </div>
        </div>

        {/* Status */}
        <div className="bg-gray-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Status</h3>
          <div className="flex gap-6">
            <label className="flex items-center gap-2 text-white">
              <input
                type="checkbox"
                name="isPublished"
                checked={formData.isPublished}
                onChange={handleChange}
                className="w-4 h-4"
              />
              Published
            </label>
            <label className="flex items-center gap-2 text-white">
              <input
                type="checkbox"
                name="isFeatured"
                checked={formData.isFeatured}
                onChange={handleChange}
                className="w-4 h-4"
              />
              Featured
            </label>
          </div>
        </div>

        {/* Submit */}
        <div className="flex gap-4">
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
          >
            {loading ? 'Creating...' : 'Create Movie'}
          </button>
          <Link
            href="/admin/movies"
            className="px-6 py-2 bg-gray-700 text-white rounded-lg hover:bg-gray-600"
          >
            Cancel
          </Link>
        </div>
      </form>
    </div>
  );
}