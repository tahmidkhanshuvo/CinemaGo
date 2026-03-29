'use client';

import { useEffect, useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import Link from 'next/link';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

export default function EditMoviePage() {
  const router = useRouter();
  const params = useParams();
  const movieId = params.id as string;
  const { user, isAuthenticated } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
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
      return;
    }

    const fetchMovie = async () => {
      try {
        const response = await api.getAdminMovieById(parseInt(movieId)) as any;
        if (response.success && response.data) {
          const movie = response.data;
          setFormData({
            title: movie.title || '',
            slug: movie.slug || '',
            description: movie.description || '',
            durationMinutes: movie.durationMinutes || 120,
            releaseDate: movie.releaseDate ? movie.releaseDate.split('T')[0] : '',
            language: movie.language || 'English',
            country: movie.country || 'USA',
            genre: movie.genre || 'Action',
            ageRating: movie.ageRating || 'PG-13',
            posterUrl: movie.posterUrl || '',
            bannerUrl: movie.bannerUrl || '',
            trailerUrl: movie.trailerUrl || '',
            videoUrl: movie.videoUrl || '',
            movieType: movie.movieType || 'CINEMA',
            ottPrice: movie.ottPrice || 0,
            isPublished: movie.isPublished ?? true,
            isFeatured: movie.isFeatured ?? false
          });
        }
      } catch (error) {
        console.error('Error fetching movie:', error);
        setError('Failed to load movie');
      } finally {
        setFetching(false);
      }
    };

    fetchMovie();
  }, [isAuthenticated, user, router, movieId]);

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
    setSuccess('');

    try {
      const response = await api.updateMovie(parseInt(movieId), formData) as any;
      if (response.success) {
        setSuccess('Movie updated successfully!');
        setTimeout(() => router.push('/admin/movies'), 1500);
      } else {
        setError(response.message || 'Failed to update movie');
      }
    } catch (err: any) {
      setError(err.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!confirm('Are you sure you want to delete this movie? This action cannot be undone.')) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await api.deleteMovie(parseInt(movieId)) as any;
      if (response.success) {
        router.push('/admin/movies');
      } else {
        setError(response.message || 'Failed to delete movie');
      }
    } catch (err: any) {
      setError(err.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  if (!isAuthenticated || user?.role !== 'Admin') return null;
  if (fetching) return <div className="text-white">Loading...</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
          <Link href="/admin/movies" className="text-gray-400 hover:text-white">
            ← Back
          </Link>
          <h2 className="text-2xl font-bold text-white">Edit Movie</h2>
        </div>
        <button
          onClick={handleDelete}
          disabled={loading}
          className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
        >
          Delete Movie
        </button>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-600/20 border border-red-600 text-red-400 rounded-lg">
          {error}
        </div>
      )}

      {success && (
        <div className="mb-4 p-3 bg-green-600/20 border border-green-600 text-green-400 rounded-lg">
          {success}
        </div>
      )}

      <form onSubmit={handleSubmit} className="bg-gray-800 rounded-lg p-6 space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-gray-400 text-sm mb-2">Title</label>
            <input
              type="text"
              name="title"
              value={formData.title}
              onChange={handleChange}
              required
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Slug</label>
            <input
              type="text"
              name="slug"
              value={formData.slug}
              onChange={handleChange}
              required
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div className="md:col-span-2">
            <label className="block text-gray-400 text-sm mb-2">Description</label>
            <textarea
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={4}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Duration (minutes)</label>
            <input
              type="number"
              name="durationMinutes"
              value={formData.durationMinutes}
              onChange={handleChange}
              min="1"
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Release Date</label>
            <input
              type="date"
              name="releaseDate"
              value={formData.releaseDate}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Language</label>
            <input
              type="text"
              name="language"
              value={formData.language}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Country</label>
            <input
              type="text"
              name="country"
              value={formData.country}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Genre</label>
            <select
              name="genre"
              value={formData.genre}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            >
              <option value="Action">Action</option>
              <option value="Comedy">Comedy</option>
              <option value="Drama">Drama</option>
              <option value="Horror">Horror</option>
              <option value="Thriller">Thriller</option>
              <option value="Romance">Romance</option>
              <option value="Sci-Fi">Sci-Fi</option>
              <option value="Adventure">Adventure</option>
              <option value="Animation">Animation</option>
              <option value="Documentary">Documentary</option>
            </select>
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Age Rating</label>
            <select
              name="ageRating"
              value={formData.ageRating}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            >
              <option value="G">G</option>
              <option value="PG">PG</option>
              <option value="PG-13">PG-13</option>
              <option value="R">R</option>
              <option value="NC-17">NC-17</option>
            </select>
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Movie Type</label>
            <select
              name="movieType"
              value={formData.movieType}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            >
              <option value="CINEMA">Cinema</option>
              <option value="OTT">OTT</option>
              <option value="BOTH">Both</option>
            </select>
          </div>

          {formData.movieType !== 'CINEMA' && (
            <div>
              <label className="block text-gray-400 text-sm mb-2">OTT Price ($)</label>
              <input
                type="number"
                name="ottPrice"
                value={formData.ottPrice}
                onChange={handleChange}
                min="0"
                step="0.01"
                className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
              />
            </div>
          )}

          <div>
            <label className="block text-gray-400 text-sm mb-2">Poster URL</label>
            <input
              type="url"
              name="posterUrl"
              value={formData.posterUrl}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Banner URL</label>
            <input
              type="url"
              name="bannerUrl"
              value={formData.bannerUrl}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Trailer URL</label>
            <input
              type="url"
              name="trailerUrl"
              value={formData.trailerUrl}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div>
            <label className="block text-gray-400 text-sm mb-2">Video URL (for OTT)</label>
            <input
              type="url"
              name="videoUrl"
              value={formData.videoUrl}
              onChange={handleChange}
              className="w-full px-4 py-2 bg-gray-700 text-white rounded-lg focus:ring-2 focus:ring-red-500 outline-none"
            />
          </div>

          <div className="flex items-center gap-6">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                name="isPublished"
                checked={formData.isPublished}
                onChange={handleChange}
                className="w-4 h-4 text-red-600 bg-gray-700 border-gray-600 rounded focus:ring-red-500"
              />
              <span className="text-white">Published</span>
            </label>

            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                name="isFeatured"
                checked={formData.isFeatured}
                onChange={handleChange}
                className="w-4 h-4 text-red-600 bg-gray-700 border-gray-600 rounded focus:ring-red-500"
              />
              <span className="text-white">Featured</span>
            </label>
          </div>
        </div>

        <div className="flex justify-end gap-4 pt-4 border-t border-gray-700">
          <Link
            href="/admin/movies"
            className="px-6 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
          >
            {loading ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  );
}