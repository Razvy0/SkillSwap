import { useEffect, useState } from 'react';
import { useMySkills } from '@/hooks/useSkills';
import { useCreateSwap } from '@/hooks/useSwaps';
import { Skill } from '@/services/skillService';
import { LessonCadence, LessonType, TwoWayScheduleMode } from '@/services/swapService';
import { X } from 'lucide-react';

interface Props {
    targetSkill: Skill;
    onClose: () => void;
}

export default function ProposeSwapModal({ targetSkill, onClose }: Props) {
    const { data: mySkills } = useMySkills();
    const createSwap = useCreateSwap();
    const [selectedSkillId, setSelectedSkillId] = useState<number>(0);
    const [lessonType, setLessonType] = useState<LessonType>('OneWay');
    const [requestedCadence, setRequestedCadence] = useState<LessonCadence>(
        targetSkill.lessonMode === 'RecurringOnly' || targetSkill.requiredSessions > 1 ? 'Weekly' : 'Single'
    );
    const [offeredCadence, setOfferedCadence] = useState<LessonCadence>('Single');
    const [twoWayScheduleMode, setTwoWayScheduleMode] = useState<TwoWayScheduleMode>('Separate');

    const offeringSkills = mySkills?.filter((s) => s.isOffering) ?? [];
    const isExchange = lessonType === 'Exchange';
    const selectedSkill = offeringSkills.find((s) => s.id === selectedSkillId);

    const requestedCadenceOptions: LessonCadence[] =
        targetSkill.lessonMode === 'SingleOnly' || targetSkill.requiredSessions === 1
            ? ['Single']
            : targetSkill.lessonMode === 'RecurringOnly' || targetSkill.requiredSessions > 1
                ? ['Weekly']
                : ['Single', 'Weekly'];

    const offeredCadenceOptions: LessonCadence[] = selectedSkill
        ? selectedSkill.lessonMode === 'SingleOnly' || selectedSkill.requiredSessions === 1
            ? ['Single']
            : selectedSkill.lessonMode === 'RecurringOnly' || selectedSkill.requiredSessions > 1
                ? ['Weekly']
                : ['Single', 'Weekly']
        : ['Single', 'Weekly'];

    const isConsecutiveDisabled =
        !selectedSkill ||
        selectedSkill.requiredSessions !== targetSkill.requiredSessions ||
        offeredCadence !== requestedCadence;

    useEffect(() => {
        if (!selectedSkill) return;
        const defaultCadence = selectedSkill.lessonMode === 'RecurringOnly' || selectedSkill.requiredSessions > 1
            ? 'Weekly'
            : 'Single';
        setOfferedCadence(defaultCadence);
    }, [selectedSkill?.id, selectedSkill?.lessonMode]);

    useEffect(() => {
        if (isConsecutiveDisabled && twoWayScheduleMode === 'Consecutive') {
            setTwoWayScheduleMode('Separate');
        }
    }, [isConsecutiveDisabled, twoWayScheduleMode]);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const payload = {
            requestedSkillId: targetSkill.id,
            lessonType,
            requestedCadence,
            twoWayScheduleMode,
            ...(isExchange ? { offeredSkillId: selectedSkillId } : {}),
            ...(isExchange ? { offeredCadence } : {}),
        };
        createSwap.mutate(
            payload,
            { onSuccess: () => onClose() }
        );
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold text-gray-900">Propose Swap</h2>
                    <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
                        <X size={20} />
                    </button>
                </div>

                <div className="mb-4 p-3 bg-blue-50 rounded-lg">
                    <p className="text-sm text-gray-500">You want</p>
                    <p className="font-semibold text-gray-900">{targetSkill.title}</p>
                    <p className="text-sm text-gray-500">from {targetSkill.userFullName}</p>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <p className="block text-sm font-medium text-gray-700 mb-1">Lesson type</p>
                        <div className="grid grid-cols-1 gap-2">
                            <label className={`flex items-start gap-3 rounded-lg border px-3 py-2 cursor-pointer transition-colors ${lessonType === 'OneWay' ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:border-gray-300'}`}>
                                <input
                                    type="radio"
                                    name="lessonType"
                                    value="OneWay"
                                    checked={lessonType === 'OneWay'}
                                    onChange={() => setLessonType('OneWay')}
                                    className="mt-1"
                                />
                                <div>
                                    <p className="text-sm font-medium text-gray-900">One-way lesson</p>
                                    <p className="text-xs text-gray-500">You learn, the other user teaches. 1 credit is held and earned on completion.</p>
                                </div>
                            </label>
                            <label className={`flex items-start gap-3 rounded-lg border px-3 py-2 cursor-pointer transition-colors ${lessonType === 'Exchange' ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:border-gray-300'}`}>
                                <input
                                    type="radio"
                                    name="lessonType"
                                    value="Exchange"
                                    checked={lessonType === 'Exchange'}
                                    onChange={() => setLessonType('Exchange')}
                                    className="mt-1"
                                />
                                <div>
                                    <p className="text-sm font-medium text-gray-900">Skill exchange</p>
                                    <p className="text-xs text-gray-500">You both teach each other. No credits are transferred.</p>
                                </div>
                            </label>
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Requested skill cadence</label>
                        <select
                            value={requestedCadence}
                            onChange={(e) => setRequestedCadence(e.target.value as LessonCadence)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none"
                        >
                            {requestedCadenceOptions.map((cadence) => (
                                <option key={cadence} value={cadence}>
                                    {cadence === 'Single' ? 'Single session' : `Weekly (${targetSkill.requiredSessions} sessions)`}
                                </option>
                            ))}
                        </select>
                        <p className="mt-1 text-xs text-gray-500">
                            This skill requires {targetSkill.requiredSessions} session{targetSkill.requiredSessions === 1 ? '' : 's'}.
                        </p>
                    </div>

                    {isExchange && (
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Select one of your skills
                            </label>
                            {offeringSkills.length > 0 ? (
                                <select
                                    value={selectedSkillId}
                                    onChange={(e) => setSelectedSkillId(Number(e.target.value))}
                                    required={isExchange}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none"
                                >
                                    <option value={0} disabled>Select a skill to offer</option>
                                    {offeringSkills.map((s) => (
                                        <option key={s.id} value={s.id}>
                                            {s.title} ({s.categoryName})
                                        </option>
                                    ))}
                                </select>
                            ) : (
                                <p className="text-sm text-gray-500">
                                    You have no skills listed as "Offering". Add one from your Profile first.
                                </p>
                            )}
                        </div>
                    )}

                    {isExchange && selectedSkill && (
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Offered skill cadence</label>
                            <select
                                value={offeredCadence}
                                onChange={(e) => setOfferedCadence(e.target.value as LessonCadence)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none"
                            >
                                {offeredCadenceOptions.map((cadence) => (
                                    <option key={cadence} value={cadence}>
                                        {cadence === 'Single' ? 'Single session' : `Weekly (${selectedSkill.requiredSessions} sessions)`}
                                    </option>
                                ))}
                            </select>
                        </div>
                    )}

                    {isExchange && (
                        <div>
                            <p className="block text-sm font-medium text-gray-700 mb-1">Two-way scheduling</p>
                            <div className="grid grid-cols-1 gap-2">
                                <label className={`flex items-start gap-3 rounded-lg border px-3 py-2 cursor-pointer transition-colors ${twoWayScheduleMode === 'Separate' ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:border-gray-300'}`}>
                                    <input
                                        type="radio"
                                        name="twoWayScheduleMode"
                                        value="Separate"
                                        checked={twoWayScheduleMode === 'Separate'}
                                        onChange={() => setTwoWayScheduleMode('Separate')}
                                        className="mt-1"
                                    />
                                    <div>
                                        <p className="text-sm font-medium text-gray-900">Separate schedules</p>
                                        <p className="text-xs text-gray-500">Each skill is scheduled independently.</p>
                                    </div>
                                </label>
                                <label className={`flex items-start gap-3 rounded-lg border px-3 py-2 cursor-pointer transition-colors ${twoWayScheduleMode === 'Consecutive' ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:border-gray-300'} ${isConsecutiveDisabled ? 'opacity-50 cursor-not-allowed' : ''}`}>
                                    <input
                                        type="radio"
                                        name="twoWayScheduleMode"
                                        value="Consecutive"
                                        checked={twoWayScheduleMode === 'Consecutive'}
                                        onChange={() => !isConsecutiveDisabled && setTwoWayScheduleMode('Consecutive')}
                                        className="mt-1"
                                        disabled={isConsecutiveDisabled}
                                    />
                                    <div>
                                        <p className="text-sm font-medium text-gray-900">Consecutive sessions</p>
                                        <p className="text-xs text-gray-500">Back-to-back sessions, same cadence and session count.</p>
                                    </div>
                                </label>
                            </div>
                        </div>
                    )}

                    {isExchange && (
                        <div className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Offered skill cadence</label>
                                <select
                                    value={offeredCadence}
                                    onChange={(e) => setOfferedCadence(e.target.value as LessonCadence)}
                                    disabled={!selectedSkill}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none disabled:bg-gray-100"
                                >
                                    {offeredCadenceOptions.map((cadence) => (
                                        <option key={cadence} value={cadence}>
                                            {cadence === 'Single'
                                                ? 'Single session'
                                                : `Weekly (${selectedSkill?.requiredSessions ?? 0} sessions)`}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Two-way scheduling</label>
                                <select
                                    value={twoWayScheduleMode}
                                    onChange={(e) => setTwoWayScheduleMode(e.target.value as TwoWayScheduleMode)}
                                    disabled={isConsecutiveDisabled}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 outline-none disabled:bg-gray-100"
                                >
                                    <option value="Separate">Separate schedules</option>
                                    <option value="Consecutive">Consecutive sessions</option>
                                </select>
                                {isConsecutiveDisabled && (
                                    <p className="mt-1 text-xs text-gray-500">
                                        Consecutive scheduling requires matching cadences and session counts.
                                    </p>
                                )}
                            </div>
                        </div>
                    )}

                    {createSwap.isError && (
                        <p className="text-sm text-red-600">
                            {(createSwap.error as any)?.response?.data?.message || 'Failed to create swap'}
                        </p>
                    )}

                    <button
                        type="submit"
                        disabled={createSwap.isPending || (isExchange && selectedSkillId === 0)}
                        className="w-full py-2.5 bg-primary-600 text-white rounded-lg font-medium hover:bg-primary-700 disabled:opacity-50 transition-colors"
                    >
                        {createSwap.isPending ? 'Sending...' : 'Send Swap Request'}
                    </button>
                </form>
            </div>
        </div>
    );
}
