import { useState } from 'react';
import { useMe, useUpdateProfile } from '@/hooks/useUsers';
import { useMySkills, useDeleteSkill } from '@/hooks/useSkills';
import AddSkillModal from '@/components/AddSkillModal';
import TimeTransactionHistory from '@/components/TimeTransactionHistory';
import { Plus, Trash2, Pencil, Check, X } from 'lucide-react';

export default function ProfilePage() {
  const { data: profile, isLoading } = useMe();
  const { data: mySkills } = useMySkills();
  const deleteSkill = useDeleteSkill();
  const updateProfile = useUpdateProfile();
  const [deleteError, setDeleteError] = useState('');

  const [showAddSkill, setShowAddSkill] = useState(false);
  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editBio, setEditBio] = useState('');

  if (isLoading) return <p className="text-gray-500">Loading profile...</p>;
  if (!profile) return <p className="text-gray-500">Profile not found.</p>;

  const startEditing = () => {
    setEditName(profile.fullName);
    setEditBio(profile.bio ?? '');
    setEditing(true);
  };

  const saveProfile = () => {
    updateProfile.mutate(
      { fullName: editName, bio: editBio },
      { onSuccess: () => setEditing(false) }
    );
  };

  return (
    <div>
      {/* Profile Card */}
      <div className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
        {editing ? (
          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
              <input
                type="text"
                value={editName}
                onChange={(e) => setEditName(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Bio</label>
              <textarea
                value={editBio}
                onChange={(e) => setEditBio(e.target.value)}
                rows={3}
                maxLength={500}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none resize-none ${editBio.length >= 500 ? 'border-red-400' : 'border-gray-300'}`}
                placeholder="Tell us about yourself..."
              />
              <div className="flex justify-between mt-1">
                {editBio.length >= 500 && (
                  <p className="text-xs text-red-500">Bio must be at most 500 characters</p>
                )}
                <p className={`text-xs ml-auto ${editBio.length >= 500 ? 'text-red-500' : 'text-gray-400'}`}>{editBio.length}/500</p>
              </div>
            </div>
            <div className="flex gap-2">
              <button
                onClick={saveProfile}
                disabled={updateProfile.isPending}
                className="flex items-center gap-1 px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-medium hover:bg-primary-700 disabled:opacity-50"
              >
                <Check size={16} /> Save
              </button>
              <button
                onClick={() => setEditing(false)}
                className="flex items-center gap-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-200"
              >
                <X size={16} /> Cancel
              </button>
            </div>
          </div>
        ) : (
          <>
            <div className="flex justify-between items-start">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">{profile.fullName}</h1>
                <p className="text-gray-500">{profile.email}</p>
                {profile.bio && <p className="mt-2 text-gray-600">{profile.bio}</p>}
              </div>
              <button
                onClick={startEditing}
                className="flex items-center gap-1 px-3 py-1.5 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200"
              >
                <Pencil size={14} /> Edit
              </button>
            </div>
            <div className="flex gap-6 mt-4">
              <div>
                <span className="text-2xl font-bold text-primary-600">{profile.timeBalance}</span>
                <span className="text-sm text-gray-500 ml-1">Time Credits</span>
              </div>
              <div>
                <span className="text-2xl font-bold text-yellow-500">{profile.rating.toFixed(1)}</span>
                <span className="text-sm text-gray-500 ml-1">Rating</span>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Skills Section */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-lg font-semibold text-gray-900">Your Skills</h2>
        <button
          onClick={() => {
            setDeleteError('');
            setShowAddSkill(true);
          }}
          className="flex items-center gap-1 px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-medium hover:bg-primary-700 transition-colors"
        >
          <Plus size={16} /> Add Skill
        </button>
      </div>

      {deleteError && (
        <p className="mb-3 text-sm text-red-600">{deleteError}</p>
      )}

      {mySkills && mySkills.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {mySkills.map((s) => (
            <div key={s.id} className="bg-white border border-gray-200 rounded-lg p-4 flex justify-between items-center">
              <div>
                <p className="font-medium text-gray-900">{s.title}</p>
                <p className="text-sm text-gray-500">{s.categoryName}</p>
                {s.description && (
                  <p className="text-sm text-gray-400 mt-1 line-clamp-1">{s.description}</p>
                )}
              </div>
              <div className="flex items-center gap-2">
                <span
                  className={`text-xs font-medium px-2.5 py-1 rounded-full ${s.isOffering ? 'bg-green-100 text-green-700' : 'bg-blue-100 text-blue-700'
                    }`}
                >
                  {s.isOffering ? 'Offering' : 'Seeking'}
                </span>
                <button
                  onClick={() =>
                    deleteSkill.mutate(s.id, {
                      onSuccess: () => setDeleteError(''),
                      onError: (error: any) => {
                        const apiMessage = error?.response?.data?.message as string | undefined;
                        if (apiMessage?.toLowerCase().includes('active swap')) {
                          setDeleteError('This skill is part of an active swap and can’t be removed right now.');
                          return;
                        }
                        setDeleteError(apiMessage || 'Failed to remove skill');
                      }
                    })
                  }
                  className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title="Remove skill"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-gray-500">No skills added yet. Click "Add Skill" to get started!</p>
      )}

      {/* Transaction History Section */}
      <div className="mt-8">
        <TimeTransactionHistory />
      </div>

      {showAddSkill && <AddSkillModal onClose={() => setShowAddSkill(false)} />}
    </div>
  );
}
