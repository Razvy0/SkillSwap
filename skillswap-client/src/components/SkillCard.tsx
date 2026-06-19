import { Skill } from '@/services/skillService';

interface Props {
  skill: Skill;
  onSwap?: (skill: Skill) => void;
  showActions?: boolean;
}

export default function SkillCard({ skill, onSwap, showActions = false }: Props) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start mb-3">
        <div>
          <h3 className="font-semibold text-gray-900">{skill.title}</h3>
          <p className="text-sm text-gray-500">{skill.userFullName}</p>
        </div>
        <span
          className={`text-xs font-medium px-2.5 py-1 rounded-full ${skill.isOffering
            ? 'bg-green-100 text-green-700'
            : 'bg-blue-100 text-blue-700'
            }`}
        >
          {skill.isOffering ? 'Offering' : 'Seeking'}
        </span>
      </div>
      {skill.description && (
        <p className="text-sm text-gray-600 mb-3 line-clamp-2">{skill.description}</p>
      )}
      <div className="flex items-center justify-between">
        <div className="flex gap-2">
          <span className="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">
            {skill.categoryName}
          </span>
          <span className="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">
            {skill.proficiencyLevel}
          </span>
        </div>
        {showActions && onSwap && skill.isOffering && (
          <button
            onClick={() => onSwap(skill)}
            className="text-sm font-medium text-primary-600 hover:text-primary-800"
          >
            Propose Swap
          </button>
        )}
      </div>
    </div>
  );
}
