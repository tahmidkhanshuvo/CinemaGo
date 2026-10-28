const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

export interface Movie {
  id: number; title: string; slug: string; movieType: string; ottPrice: number;
  posterUrl?: string; bannerUrl?: string; description?: string; genre?: string; durationMinutes?: number;
}
export interface HomeData {
  featured: Movie[]; ottMovies: Movie[]; cinemaMovies: Movie[]; nowShowing: Movie[]; comingSoon: Movie[];
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]>;
}

async function fetchApi<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
    signal: options.signal ?? AbortSignal.timeout(90_000),
  });

  const data = await response.json().catch(() => ({ message: 'The server returned an unexpected response. Please try again.' }));

  if (!response.ok) {
    throw new Error(data.message || 'Something went wrong');
  }

  return data;
}

export const api = {
  // Auth
  register: (data: {
    fullName: string;
    email: string;
    password: string;
    confirmPassword: string;
    phone?: string;
  }) => fetchApi('/auth/register', { method: 'POST', body: JSON.stringify(data) }),

  login: (data: { email: string; password: string }) =>
    fetchApi('/auth/login', { method: 'POST', body: JSON.stringify(data) }),

  getCurrentUser: () => fetchApi('/auth/me'),

  // Movies
  getMovieById: (id: number) => fetchApi(`/movies/${id}`),

  // Movies
  getHomeData: () => fetchApi<HomeData>('/movies/home'),

  getMovies: (params?: { search?: string; movieType?: string; genre?: string; page?: number; pageSize?: number }) => {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return fetchApi<Movie[]>(`/movies${query ? `?${query}` : ''}`);
  },

  getMovieBySlug: (slug: string) => fetchApi(`/movies/${slug}`),

  // Cinema
  getShowTimes: (params?: { movieId?: number; hallId?: number; date?: string }) => {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return fetchApi(`/cinema/showtimes${query ? `?${query}` : ''}`);
  },

  getShowTimeWithSeats: (id: number) => fetchApi(`/cinema/showtimes/${id}`),

  bookTickets: (data: { showTimeId: number; seatIds: number[]; paymentMethod: string }) =>
    fetchApi('/cinema/book', { method: 'POST', body: JSON.stringify(data) }),

  // OTT
  purchaseMovie: (movieId: number) =>
    fetchApi(`/ott/${movieId}/purchase`, { method: 'POST' }),

  getMyLibrary: () => fetchApi('/ott/purchases'),

  watchMovie: (movieId: number) => fetchApi(`/ott/${movieId}/watch`),

  // Ratings
  rateMovie: (data: { movieId: number; score: number }) =>
    fetchApi('/ratings', { method: 'POST', body: JSON.stringify(data) }),

  // Profile
  getProfile: () => fetchApi('/profile'),

  changePassword: (data: { currentPassword: string; newPassword: string; confirmPassword: string }) =>
    fetchApi('/profile/password', { method: 'PUT', body: JSON.stringify(data) }),

  updateProfile: (data: { fullName?: string; phone?: string }) =>
    fetchApi('/profile', { method: 'PUT', body: JSON.stringify(data) }),

  getPurchases: () => fetchApi('/profile/purchases'),

  getTickets: () => fetchApi('/profile/purchases'),

  // Admin
  getAdminDashboard: () => fetchApi('/admin/dashboard'),

  getAdminMovies: (params?: { search?: string; movieType?: string; isPublished?: boolean }) => {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return fetchApi(`/admin/movies${query ? `?${query}` : ''}`);
  },

  getAdminMovieById: (id: number) => fetchApi(`/admin/movies/${id}`),

  createMovie: (data: any) =>
    fetchApi('/admin/movies', { method: 'POST', body: JSON.stringify(data) }),

  updateMovie: (id: number, data: any) =>
    fetchApi(`/admin/movies/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deleteMovie: (id: number) =>
    fetchApi(`/admin/movies/${id}`, { method: 'DELETE' }),

  getAdminHalls: () => fetchApi('/admin/halls'),

  createHall: (data: any) =>
    fetchApi('/admin/halls', { method: 'POST', body: JSON.stringify(data) }),

  updateHall: (id: number, data: any) =>
    fetchApi(`/admin/halls/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deleteHall: (id: number) =>
    fetchApi(`/admin/halls/${id}`, { method: 'DELETE' }),

  getAdminShowTimes: (params?: { movieId?: number; hallId?: number; date?: string }) => {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return fetchApi(`/admin/showtimes${query ? `?${query}` : ''}`);
  },

  createShowTime: (data: any) =>
    fetchApi('/admin/showtimes', { method: 'POST', body: JSON.stringify(data) }),

  deleteShowTime: (id: number) =>
    fetchApi(`/admin/showtimes/${id}`, { method: 'DELETE' }),

  updateShowTime: (id: number, data: any) =>
    fetchApi(`/admin/showtimes/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  getAdminBookings: (params?: { status?: string; fromDate?: string; toDate?: string }) => {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return fetchApi(`/admin/bookings${query ? `?${query}` : ''}`);
  },

  cancelBooking: (id: number) =>
    fetchApi(`/admin/bookings/${id}/cancel`, { method: 'PUT' }),
};

export default api;
