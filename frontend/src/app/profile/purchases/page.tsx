'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Purchase {
  id: number;
  movieId: number;
  movieTitle: string;
  moviePosterUrl: string;
  amount: number;
  purchasedAt: string;
}

export default function PurchasesPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [purchases, setPurchases] = useState<Purchase[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }

    const fetchPurchases = async () => {
      try {
        const response = await api.getPurchases() as any;
        if (response.success && response.data) {
          setPurchases(response.data);
        }
      } catch (error) {
        console.error('Error fetching purchases:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPurchases();
  }, [isAuthenticated, router]);

  if (!isAuthenticated) return null;

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold text-white mb-6">My Purchases</h2>

      {loading ? (
        <div className="text-white">Loading...</div>
      ) : purchases.length === 0 ? (
        <div className="bg-gray-800 rounded-lg p-8 text-center">
          <p className="text-gray-400">No purchases yet</p>
          <a href="/ott" className="text-red-500 hover:text-red-400 mt-2 inline-block">
            Browse OTT movies
          </a>
        </div>
      ) : (
        <div className="space-y-4">
          {purchases.map((purchase) => (
            <div key={purchase.id} className="bg-gray-800 rounded-lg p-4 flex items-center gap-4">
              {purchase.moviePosterUrl && (
                <img 
                  src={purchase.moviePosterUrl} 
                  alt={purchase.movieTitle}
                  className="w-20 h-28 object-cover rounded"
                />
              )}
              <div className="flex-1">
                <h3 className="text-white font-semibold">{purchase.movieTitle}</h3>
                <p className="text-gray-400 text-sm">
                  Purchased on {new Date(purchase.purchasedAt).toLocaleDateString()}
                </p>
              </div>
              <div className="text-right">
                <p className="text-white font-bold">${purchase.amount}</p>
                <a 
                  href={`/watch/${purchase.movieId}`}
                  className="text-sm text-green-500 hover:text-green-400"
                >
                  Watch Now →
                </a>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}