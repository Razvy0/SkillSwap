import { useGenerateRecommendations } from '@/hooks/useRecommendations';
import { useState } from 'react';
import type { RecommendationMatch } from '@/services/recommendationService';
import { Sparkles, Medal, TrendingUp } from 'lucide-react';

const podiumHeights = ['h-52', 'h-40', 'h-36'];
const podiumRanks = ['#1', '#2', '#3'];

const podiumGridOrder = ['order-1 md:order-2', 'order-2 md:order-1', 'order-3'];

export default function RecommendationsPage() {
  const [hasGenerated, setHasGenerated] = useState(false);
  const { mutate, data, isPending, isError, error } = useGenerateRecommendations();
  const matches = data?.matches ?? [];

  const handleGenerate = () => {
    setHasGenerated(true);
    mutate();
  };

  return (
    <div className="space-y-8">
      <div className="bg-gradient-to-r from-amber-50 via-white to-sky-50 border border-gray-200 rounded-2xl p-6 shadow-sm">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div>
            <div className="flex items-center gap-2 text-amber-600 mb-2">
              <Sparkles size={18} />
              <span className="text-xs font-semibold tracking-wide uppercase">Smart Matching</span>
            </div>
            <h1 className="text-2xl font-bold text-gray-900">Recommendations</h1>
            <p className="text-gray-600 mt-1">
              Generate your top 3 best lesson matches based on skills, swaps, and ratings.
            </p>
          </div>
          <button
            onClick={handleGenerate}
            disabled={isPending}
            className="inline-flex items-center justify-center gap-2 px-5 py-2.5 rounded-lg bg-gray-900 text-white text-sm font-semibold hover:bg-gray-800 disabled:opacity-60"
          >
            {isPending ? 'Generating...' : 'Generate recommendations'}
          </button>
        </div>
      </div>

      {isError ? (
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 text-sm">
          {(error as Error)?.message ?? 'Failed to generate recommendations.'}
        </div>
      ) : null}

      {matches.length > 0 ? (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
            {[0, 1, 2].map((slotIndex) => {
              const match = matches[slotIndex];
              if (!match) return null;
              
              return (
                <div 
                  key={match.userId} 
                  // Am integrat clasa de ordine vizuală
                  className={`bg-white border border-gray-200 rounded-xl p-4 shadow-sm ${podiumGridOrder[slotIndex]}`}
                >
                  <div className="flex items-center justify-between mb-3">
                    <span className="text-xs font-semibold text-gray-500">{podiumRanks[slotIndex]}</span>
                    <Medal size={16} className="text-amber-500" />
                  </div>
                  <div className="flex items-center gap-2">
                    <div className={`w-full rounded-lg ${podiumHeights[slotIndex]} bg-gradient-to-t from-amber-100 to-white flex items-end justify-center`}>
                      <span className="mb-3 text-sm font-semibold text-gray-700 text-center px-1">{match.fullName}</span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
            {matches.map((match) => (
              <MatchCard key={match.userId} match={match} />
            ))}
          </div>
        </div>
      ) : (
        <div className="border border-dashed border-gray-300 rounded-xl p-8 text-center text-gray-500">
          <TrendingUp size={22} className="mx-auto mb-3" />
          {hasGenerated
            ? 'Sorry, there are not enough users to generate recommendations yet.'
            : 'Generate your recommendations to see your top 3 matches.'}
        </div>
      )}
    </div>
  );
}

function MatchCard({ match }: { match: RecommendationMatch }) {
  const topSkills = match.skills.slice(0, 3);

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm space-y-4">
      <div>
        <h3 className="text-lg font-semibold text-gray-900">{match.fullName}</h3>
        <p className="text-sm text-gray-500">{match.bio || 'No bio yet.'}</p>
      </div>
      <div className="flex flex-wrap gap-2">
        {topSkills.length > 0 ? (
          topSkills.map((skill) => (
            <span
              key={skill.id}
              className={`text-xs font-semibold px-2 py-1 rounded-full ${
                skill.isOffering ? 'bg-emerald-100 text-emerald-700' : 'bg-sky-100 text-sky-700'
              }`}
            >
              {skill.title}
            </span>
          ))
        ) : (
          <span className="text-xs text-gray-400">No skills listed.</span>
        )}
      </div>
      <div className="text-sm text-gray-600 space-y-1">
        <p>Rating: {match.rating ? match.rating.toFixed(1) : 'Unrated'} ({match.reviewCount} reviews)</p>
        <p>Similarity: {(match.similarity * 100).toFixed(1)}%</p>
      </div>
      <div className="text-sm text-gray-700 bg-amber-50 border border-amber-100 rounded-lg p-3">
        {match.reason}
      </div>
    </div>
  );
}