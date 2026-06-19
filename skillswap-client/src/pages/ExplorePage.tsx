import { useState } from 'react';
import { useSkills } from '@/hooks/useSkills';
import { useSearchUsers } from '@/hooks/useUsers';
import { useAuthStore } from '@/stores/authStore';
import SkillCard from '@/components/SkillCard';
import ProposeSwapModal from '@/components/ProposeSwapModal';
import { Skill } from '@/services/skillService';
import { Search, Users, Layers } from 'lucide-react';
import { Link } from 'react-router-dom';

type Tab = 'skills' | 'people';

export default function ExplorePage() {
  const userId = useAuthStore((s) => s.userId);
  const [tab, setTab] = useState<Tab>('skills');
  const [search, setSearch] = useState('');
  const [offeringFilter, setOfferingFilter] = useState<boolean | undefined>(true);
  const [swapTarget, setSwapTarget] = useState<Skill | null>(null);

  // Pagination states
  const [skillsPage, setSkillsPage] = useState(1);
  const [usersPage, setUsersPage] = useState(1);
  const pageSize = 12;

  const { data: skillsData, isLoading: skillsLoading } = useSkills({
    search: search || undefined,
    isOffering: offeringFilter,
    page: skillsPage,
    pageSize,
  });

  const { data: usersData, isLoading: usersLoading } = useSearchUsers(
    tab === 'people' && search ? { name: search, skill: search, page: usersPage, pageSize } : undefined
  );

  // Filter out current user's own skills from the fetched chunk
  const filteredSkills = skillsData?.items?.filter((s) => s.userId !== userId);
  const users = usersData?.items;
  
  // Calculate total pages
  const totalSkillsPages = Math.ceil((skillsData?.totalCount || 0) / pageSize);
  const totalUsersPages = Math.ceil((usersData?.totalCount || 0) / pageSize);

  const handleTabChange = (newTab: Tab) => {
    setTab(newTab);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    setSkillsPage(1);
    setUsersPage(1);
  };

  const handleFilterChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const v = e.target.value;
    setOfferingFilter(v === 'all' ? undefined : v === 'offering');
    setSkillsPage(1);
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Explore</h1>

      {/* Tabs */}
      <div className="flex gap-1 bg-gray-100 p-1 rounded-lg w-fit mb-6">
        <button
          onClick={() => handleTabChange('skills')}
          className={`flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition-colors ${tab === 'skills' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'
            }`}
        >
          <Layers size={16} /> Skills
        </button>
        <button
          onClick={() => handleTabChange('people')}
          className={`flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition-colors ${tab === 'people' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'
            }`}
        >
          <Users size={16} /> People
        </button>
      </div>

      {/* Search */}
      <div className="flex gap-4 mb-6">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} />
          <input
            type="text"
            placeholder={tab === 'skills' ? 'Search skills...' : 'Search by name or skill...'}
            value={search}
            onChange={handleSearchChange}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none"
          />
        </div>
        {tab === 'skills' && (
          <select
            value={offeringFilter === undefined ? 'all' : offeringFilter ? 'offering' : 'seeking'}
            onChange={handleFilterChange}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none"
          >
            <option value="all">All</option>
            <option value="offering">Offering</option>
            <option value="seeking">Seeking</option>
          </select>
        )}
      </div>

      {/* Skills tab */}
      {tab === 'skills' && (
        <>
          {skillsLoading ? (
            <p className="text-gray-500">Loading skills...</p>
          ) : filteredSkills && filteredSkills.length > 0 ? (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredSkills.map((skill) => (
                  <SkillCard
                    key={skill.id}
                    skill={skill}
                    showActions
                    onSwap={(s) => setSwapTarget(s)}
                  />
                ))}
              </div>
              {totalSkillsPages > 1 && (
                <div className="flex justify-center items-center gap-4 mt-8">
                  <button
                    onClick={() => setSkillsPage((p) => Math.max(1, p - 1))}
                    disabled={skillsPage === 1}
                    className="px-4 py-2 bg-white border border-gray-300 rounded-md text-sm font-medium text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                  >
                    Previous
                  </button>
                  <span className="text-sm text-gray-600">
                    Page {skillsPage} of {totalSkillsPages}
                  </span>
                  <button
                    onClick={() => setSkillsPage((p) => Math.min(totalSkillsPages, p + 1))}
                    disabled={skillsPage === totalSkillsPages}
                    className="px-4 py-2 bg-white border border-gray-300 rounded-md text-sm font-medium text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                  >
                    Next
                  </button>
                </div>
              )}
            </>
          ) : (
            <p className="text-gray-500">No skills found.</p>
          )}
        </>
      )}

      {/* People tab */}
      {tab === 'people' && (
        <>
          {!search ? (
            <p className="text-gray-500">Type a name or skill to search for people.</p>
          ) : usersLoading ? (
            <p className="text-gray-500">Searching...</p>
          ) : users && users.length > 0 ? (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {users
                  .filter((u) => u.id !== userId)
                  .map((user) => (
                    <Link
                      key={user.id}
                      to={`/users/${user.id}`}
                      className="block bg-white border border-gray-200 rounded-xl p-5 hover:shadow-md transition-shadow"
                    >
                      <div className="flex justify-between items-start mb-3">
                        <div>
                          <h3 className="font-semibold text-gray-900">{user.fullName}</h3>
                          {user.bio && (
                            <p className="text-sm text-gray-500 mt-1 line-clamp-2">{user.bio}</p>
                          )}
                        </div>
                        <div className="flex items-center gap-1 text-yellow-500 text-sm font-medium">
                          {user.rating.toFixed(1)} *
                        </div>
                      </div>
                      {user.skills.length > 0 && (
                        <div className="flex flex-wrap gap-1.5 mb-3">
                          {user.skills.slice(0, 4).map((s) => (
                            <span
                              key={s.id}
                              className={`text-xs px-2 py-1 rounded-full ${s.isOffering ? 'bg-green-100 text-green-700' : 'bg-blue-100 text-blue-700'}`}
                            >
                              {s.title}
                            </span>
                          ))}
                          {user.skills.length > 4 && (
                            <span className="text-xs text-gray-400">
                              +{user.skills.length - 4} more
                            </span>
                          )}
                        </div>
                      )}
                      <span className="text-sm font-medium text-primary-600 hover:text-primary-800">
                        View Profile
                      </span>
                    </Link>
                  ))}
              </div>
              {totalUsersPages > 1 && (
                <div className="flex justify-center items-center gap-4 mt-8">
                  <button
                    onClick={() => setUsersPage((p) => Math.max(1, p - 1))}
                    disabled={usersPage === 1}
                    className="px-4 py-2 bg-white border border-gray-300 rounded-md text-sm font-medium text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                  >
                    Previous
                  </button>
                  <span className="text-sm text-gray-600">
                    Page {usersPage} of {totalUsersPages}
                  </span>
                  <button
                    onClick={() => setUsersPage((p) => Math.min(totalUsersPages, p + 1))}
                    disabled={usersPage === totalUsersPages}
                    className="px-4 py-2 bg-white border border-gray-300 rounded-md text-sm font-medium text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                  >
                    Next
                  </button>
                </div>
              )}
            </>
          ) : (
            <p className="text-gray-500">No users found.</p>
          )}
        </>
      )}

      {swapTarget && (
        <ProposeSwapModal targetSkill={swapTarget} onClose={() => setSwapTarget(null)} />
      )}
    </div>
  );
}
