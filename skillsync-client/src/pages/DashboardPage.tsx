import { useAuthStore } from '@/stores/authStore';
import { useSwaps } from '@/hooks/useSwaps';
import { useDashboardAnalytics } from '@/hooks/useAnalytics';
import { Clock, ArrowRight, ArrowRightLeft, Star, TrendingUp, TrendingDown, BookOpen } from 'lucide-react';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
  PieChart, Pie, Cell
} from 'recharts';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8B5CF6'];

export default function DashboardPage() {
  const fullName = useAuthStore((s) => s.fullName);
  const { data: swaps } = useSwaps();
  const { data: analytics, isLoading } = useDashboardAnalytics();

  const activeSwaps = swaps?.filter((s) => s.status === 'Pending' || s.status === 'Accepted' || s.status === 'Scheduled') ?? [];

  return (
    <div className="space-y-6">
      <div className="text-left">
        <h1 className="text-2xl font-bold text-gray-900 mb-1">Welcome back, {fullName}!</h1>
        <p className="text-gray-500 mb-2">Here's your professional analytics overview.</p>
      </div>

      {isLoading ? (
        <p className="text-gray-500">Loading your analytics dashboard...</p>
      ) : analytics ? (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <StatCard icon={<ArrowRightLeft className="text-primary-500" />} label="Completed Swaps" value={analytics.totalSwapsCompleted || 0} />
            <StatCard icon={<TrendingUp className="text-green-500" />} label="Hours Earned" value={analytics.totalHoursEarned || 0} />
            <StatCard icon={<TrendingDown className="text-red-500" />} label="Hours Spent" value={analytics.totalHoursSpent || 0} />
            <StatCard icon={<Star className="text-yellow-500" />} label="Avg. Rating" value={analytics.currentRating ? analytics.currentRating.toFixed(1) : "Unrated"} />
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Focus on Time Trend */}
            <div className="bg-white border border-gray-200 rounded-xl p-6 lg:col-span-2 shadow-sm text-left">
              <h2 className="text-lg flex justify-start font-semibold text-gray-900 mb-4 items-center gap-2">
                <Clock size={20} className="text-gray-600" />
                Time Earned vs Spent (Last 6 Months)
              </h2>
              <div className="h-72">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={analytics.timeStatsOverTime}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                    <XAxis dataKey="month" stroke="#888" fontSize={12} />
                    <YAxis stroke="#888" fontSize={12} allowDecimals={false} />
                    <Tooltip 
                      contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                    />
                    <Legend />
                    <Line type="monotone" dataKey="earned" name="Earned (Hrs)" stroke="#10b981" strokeWidth={3} dot={{ r: 4 }} activeDot={{ r: 6 }} />
                    <Line type="monotone" dataKey="spent" name="Spent (Hrs)" stroke="#ef4444" strokeWidth={3} dot={{ r: 4 }} activeDot={{ r: 6 }} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Category Pie */}
            <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm text-left">
              <h2 className="text-lg flex justify-start font-semibold text-gray-900 mb-4 items-center gap-2">
                <BookOpen size={20} className="text-gray-600" />
                Top Categories
              </h2>
              {analytics.activeCategories && analytics.activeCategories.length > 0 ? (
                <div className="h-72">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={analytics.activeCategories}
                        cx="50%"
                        cy="50%"
                        innerRadius={50}
                        outerRadius={80}
                        paddingAngle={5}
                        dataKey="swapCount"
                        nameKey="categoryName"
                      >
                        {analytics.activeCategories.map((_, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip 
                        contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                        formatter={(value, name) => [`${value} Swaps`, name]}
                      />
                      <Legend verticalAlign="bottom" height={36} iconType="circle" />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              ) : (
                <div className="h-72 flex items-center justify-center text-gray-400 text-sm">
                  Not enough data for categories yet.
                </div>
              )}
            </div>
          </div>
        </>
      ) : (
        <p className="text-gray-500">Failed to load analytics.</p>
      )}

      {/* Active swaps table from before but stylized */}
      {activeSwaps.length > 0 && (
        <div className="bg-white border text-left border-gray-200 rounded-xl p-6 shadow-sm mt-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
             <ArrowRightLeft size={20} className="text-primary-500" />
             Ongoing Lessons & Exchanges
          </h2>
          <div className="space-y-3">
            {activeSwaps.map((swap) => (
              <div key={swap.id} className="bg-gray-50 border border-gray-100 rounded-lg p-4 flex justify-between items-center transition hover:shadow-sm">
                <div>
                  {(() => {
                    const lessonType = swap.lessonType ?? 'OneWay';
                    const isExchange = lessonType === 'Exchange';
                    const teacherName = swap.teacherName ?? swap.receiverName;
                    const learnerName = swap.learnerName ?? swap.requesterName;
                    return (
                      <>
                        <p className="font-medium text-gray-900 flex items-center gap-1">
                          {isExchange ? (
                            <>
                              {swap.offeredSkillTitle} <ArrowRightLeft size={14} className="text-gray-400" /> {swap.requestedSkillTitle}
                            </>
                          ) : (
                            <>
                              {teacherName} <ArrowRight size={14} className="text-gray-400" /> {learnerName}
                            </>
                          )}
                        </p>
                        <p className="text-sm text-gray-500">
                          {isExchange ? 'Teaches each other' : `Lesson: ${swap.requestedSkillTitle}`}
                        </p>
                      </>
                    );
                  })()}
                </div>
                <div className="flex flex-col items-end gap-2">
                  <span className={`text-[11px] font-semibold px-2 py-0.5 rounded-full ${
                    (swap.lessonType ?? 'OneWay') === 'Exchange' ? 'bg-indigo-100 text-indigo-700' : 'bg-emerald-100 text-emerald-700'
                  }`}>
                    {(swap.lessonType ?? 'OneWay') === 'Exchange' ? 'Skill Exchange' : 'One-Way Lesson'}
                  </span>
                  <span className={`text-xs font-semibold px-3 py-1.5 rounded-full ${
                    swap.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' : 
                    swap.status === 'Accepted' ? 'bg-blue-100 text-blue-700' :
                    'bg-indigo-100 text-indigo-700'
                    }`}>
                    {swap.status.toUpperCase()}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function StatCard({ icon, label, value }: { icon: React.ReactNode; label: string; value: number | string }) {
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-5 flex items-center gap-4 shadow-sm hover:shadow-md transition-shadow">
      <div className="p-3 bg-gray-50 rounded-xl">{icon}</div>
      <div className="text-left">
        <p className="text-2xl font-bold text-gray-900">{value}</p>
        <p className="text-sm font-medium text-gray-500">{label}</p>
      </div>
    </div>
  );
}
