'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/lib/api';
import { useAuthStore } from '@/store/auth';

interface Hall {
  id: number;
  name: string;
  branchName: string;
  totalRows: number;
  totalSeatsPerRow: number;
  isActive: boolean;
  seatCategories: Array<{
    id: number;
    name: string;
    fixedExtraPrice: number;
  }>;
}

export default function AdminHallsPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [halls, setHalls] = useState<Hall[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingHall, setEditingHall] = useState<Hall | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    branchName: '',
    totalRows: 6,
    totalSeatsPerRow: 8,
    seatCategories: [{ name: 'Regular', fixedExtraPrice: 0 }]
  });

  useEffect(() => {
    if (!isAuthenticated || user?.role !== 'Admin') {
      router.push('/login');
      return;
    }

    const fetchHalls = async () => {
      try {
        const response = await api.getAdminHalls() as any;
        if (response.success && response.data) {
          setHalls(response.data);
        }
      } catch (error) {
        console.error('Error fetching halls:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchHalls();
  }, [isAuthenticated, user, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingHall) {
        const response = await api.updateHall(editingHall.id, formData) as any;
        if (response.success) {
          setHalls(halls.map(h => h.id === editingHall.id ? response.data : h));
          resetForm();
        }
      } else {
        const response = await api.createHall(formData) as any;
        if (response.success) {
          setHalls([...halls, response.data]);
          resetForm();
        }
      }
    } catch (error) {
      console.error('Error saving hall:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this hall?')) return;
    try {
      const response = await api.deleteHall(id) as any;
      if (response.success) {
        setHalls(halls.filter(h => h.id !== id));
      }
    } catch (error) {
      console.error('Error deleting hall:', error);
    }
  };

  const handleEdit = (hall: Hall) => {
    setEditingHall(hall);
    setFormData({
      name: hall.name,
      branchName: hall.branchName,
      totalRows: hall.totalRows,
      totalSeatsPerRow: hall.totalSeatsPerRow,
      seatCategories: hall.seatCategories?.map(c => ({ name: c.name, fixedExtraPrice: c.fixedExtraPrice })) || [{ name: 'Regular', fixedExtraPrice: 0 }]
    });
    setShowForm(true);
  };

  const resetForm = () => {
    setShowForm(false);
    setEditingHall(null);
    setFormData({
      name: '',
      branchName: '',
      totalRows: 6,
      totalSeatsPerRow: 8,
      seatCategories: [{ name: 'Regular', fixedExtraPrice: 0 }]
    });
  };

  const addCategory = () => {
    setFormData({
      ...formData,
      seatCategories: [...formData.seatCategories, { name: '', fixedExtraPrice: 0 }]
    });
  };

  const updateCategory = (index: number, field: string, value: any) => {
    const categories = [...formData.seatCategories];
    categories[index] = { ...categories[index], [field]: value };
    setFormData({ ...formData, seatCategories: categories });
  };

  const removeCategory = (index: number) => {
    setFormData({
      ...formData,
      seatCategories: formData.seatCategories.filter((_, i) => i !== index)
    });
  };

  if (!isAuthenticated || user?.role !== 'Admin') return null;

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold text-white">Cinema Halls</h2>
          <p className="text-gray-400 text-sm">Manage your cinema venues</p>
        </div>
        <button
          onClick={() => { resetForm(); setShowForm(!showForm); }}
          className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
        >
          {showForm ? 'Cancel' : '+ Add Hall'}
        </button>
      </div>

      {/* Add/Edit Hall Form */}
      {showForm && (
        <form onSubmit={handleSubmit} className="bg-gray-800 p-6 rounded-lg mb-6">
          <h3 className="text-white font-semibold mb-4">{editingHall ? 'Edit Hall' : 'Add New Hall'}</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <input
              type="text"
              placeholder="Hall Name"
              value={formData.name}
              onChange={(e) => setFormData({...formData, name: e.target.value})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              required
            />
            <input
              type="text"
              placeholder="Branch Name"
              value={formData.branchName}
              onChange={(e) => setFormData({...formData, branchName: e.target.value})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              required
            />
            <input
              type="number"
              placeholder="Total Rows"
              value={formData.totalRows}
              onChange={(e) => setFormData({...formData, totalRows: parseInt(e.target.value)})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              min="1"
              max="26"
              required
            />
            <input
              type="number"
              placeholder="Seats Per Row"
              value={formData.totalSeatsPerRow}
              onChange={(e) => setFormData({...formData, totalSeatsPerRow: parseInt(e.target.value)})}
              className="px-4 py-2 bg-gray-700 text-white rounded"
              min="1"
              max="20"
              required
            />
          </div>

          <div className="mb-4">
            <label className="block text-gray-400 text-sm mb-2">Seat Categories</label>
            {formData.seatCategories.map((cat, index) => (
              <div key={index} className="flex gap-2 mb-2">
                <input
                  type="text"
                  placeholder="Category Name"
                  value={cat.name}
                  onChange={(e) => updateCategory(index, 'name', e.target.value)}
                  className="flex-1 px-4 py-2 bg-gray-700 text-white rounded"
                  required
                />
                <input
                  type="number"
                  placeholder="Extra Price"
                  value={cat.fixedExtraPrice}
                  onChange={(e) => updateCategory(index, 'fixedExtraPrice', parseFloat(e.target.value) || 0)}
                  className="w-32 px-4 py-2 bg-gray-700 text-white rounded"
                  min="0"
                />
                {formData.seatCategories.length > 1 && (
                  <button type="button" onClick={() => removeCategory(index)} className="px-3 py-2 bg-red-600 text-white rounded hover:bg-red-700">
                    ×
                  </button>
                )}
              </div>
            ))}
            <button type="button" onClick={addCategory} className="text-blue-400 hover:text-blue-300 text-sm">
              + Add Category
            </button>
          </div>

          <div className="flex gap-4">
            <button
              type="submit"
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-500"
            >
              {editingHall ? 'Update Hall' : 'Create Hall'}
            </button>
            {editingHall && (
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
      ) : halls.length === 0 ? (
        <div className="bg-gray-800 rounded-lg p-8 text-center">
          <p className="text-gray-400 mb-4">No halls created yet</p>
          <button 
            onClick={() => setShowForm(true)}
            className="text-red-500 hover:text-red-400"
          >
            Create your first hall
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {halls.map((hall) => (
            <div key={hall.id} className="bg-gray-800 rounded-lg p-6">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="text-white font-semibold text-xl">{hall.name}</h3>
                  <p className="text-gray-400">{hall.branchName}</p>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => handleEdit(hall)}
                    className="text-blue-400 hover:text-blue-300 text-sm"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(hall.id)}
                    className="text-red-400 hover:text-red-300 text-sm"
                  >
                    Delete
                  </button>
                </div>
              </div>
              <p className="text-gray-400 mt-2">
                {hall.totalRows} rows × {hall.totalSeatsPerRow} seats = {hall.totalRows * hall.totalSeatsPerRow} seats
              </p>
              <div className="mt-2 flex flex-wrap gap-2">
                {hall.seatCategories?.map((cat) => (
                  <span key={cat.id} className="bg-gray-700 text-gray-300 text-xs px-2 py-1 rounded">
                    {cat.name} (+${cat.fixedExtraPrice})
                  </span>
                ))}
              </div>
              <span className={`mt-3 inline-block text-xs px-2 py-1 rounded ${hall.isActive ? 'bg-green-600' : 'bg-red-600'} text-white`}>
                {hall.isActive ? 'Active' : 'Inactive'}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}