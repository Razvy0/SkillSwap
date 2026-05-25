import { useEffect, useState } from 'react';
import { useSwaps, useUpdateSwapStatus, useProposeSchedule, useConfirmSchedule, useRequestScheduleChange, useValidateSession, useInvalidateSession } from '@/hooks/useSwaps';
import { useAuthStore } from '@/stores/authStore';
import { Swap } from '@/services/swapService';
import { Check, X, Clock, ArrowRight, ArrowRightLeft, Star, AlertTriangle } from 'lucide-react';
import { Link } from 'react-router-dom';
import ReviewModal from '@/components/ReviewModal';
import ReportModal from '@/components/ReportModal';
import { useHasReviewed } from '@/hooks/useReviews';

export default function SwapsPage() {
  const userId = useAuthStore((s) => s.userId);
  const setLastSeenSwapsAt = useAuthStore((s) => s.setLastSeenSwapsAt);
  const { data: swaps, isLoading } = useSwaps();

  useEffect(() => {
    if (!swaps || swaps.length === 0) return;
    const latestChangeMs = swaps.reduce((latest, swap) => {
      const changeAt = swap.updatedAt ?? swap.createdAt;
      if (!changeAt) return latest;
      const ts = new Date(changeAt).getTime();
      if (Number.isNaN(ts)) return latest;
      return Math.max(latest, ts);
    }, 0);

    if (latestChangeMs > 0) {
      setLastSeenSwapsAt(new Date(latestChangeMs).toISOString());
    }
  }, [swaps, setLastSeenSwapsAt]);

  if (isLoading) return <p className="text-gray-500">Loading swaps...</p>;

  const pending = swaps?.filter((s) => s.status === 'Pending') ?? [];
  const accepted = swaps?.filter((s) => s.status === 'Accepted') ?? [];
  const scheduled = swaps?.filter((s) => s.status === 'Scheduled') ?? [];
  const completed = swaps?.filter((s) => s.status === 'Completed') ?? [];
  const rejected = swaps?.filter((s) => s.status === 'Rejected' || s.status === 'Cancelled') ?? [];
  const disputed = swaps?.filter((s) => s.status === 'Disputed') ?? [];

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Your Swaps</h1>

      {pending.length > 0 && (
        <Section title="Pending Requests">
          {pending.map((swap) => (
            <PendingSwapCard key={swap.id} swap={swap} userId={userId!} />
          ))}
        </Section>
      )}

      {accepted.length > 0 && (
        <Section title="Accepted — Schedule Meeting">
          {accepted.map((swap) => (
            <AcceptedSwapCard key={swap.id} swap={swap} userId={userId!} />
          ))}
        </Section>
      )}

      {scheduled.length > 0 && (
        <Section title="Scheduled — Validate Meeting">
          {scheduled.map((swap) => (
            <ScheduledSwapCard key={swap.id} swap={swap} userId={userId!} />
          ))}
        </Section>
      )}

      {completed.length > 0 && (
        <Section title="Completed">
          {completed.map((swap) => (
            <CompletedSwapCard key={swap.id} swap={swap} userId={userId!} />
          ))}
        </Section>
      )}

      {rejected.length > 0 && (
        <Section title="Rejected / Cancelled">
          {rejected.map((swap) => (
            <SwapRow key={swap.id} swap={swap} userId={userId!}>
              <span className="text-xs font-medium px-2.5 py-1 rounded-full bg-red-100 text-red-700">{swap.status}</span>
            </SwapRow>
          ))}
        </Section>
      )}

      {disputed.length > 0 && (
        <Section title="Disputed (Under Review)">
          {disputed.map((swap) => (
            <SwapRow key={swap.id} swap={swap} userId={userId!}>
              <span className="flex items-center gap-1 text-xs font-medium px-2.5 py-1 rounded-full bg-orange-100 text-orange-800">
                <AlertTriangle size={12} /> Disputed
              </span>
            </SwapRow>
          ))}
        </Section>
      )}

      {(!swaps || swaps.length === 0) && (
        <p className="text-gray-500">No swaps yet. Explore skills to get started!</p>
      )}
    </div>
  );
}

/* ─── Pending: Accept / Reject ─── */
function PendingSwapCard({ swap, userId }: { swap: Swap; userId: string }) {
  const updateStatus = useUpdateSwapStatus();
  const isReceiver = swap.receiverId === userId;

  return (
    <SwapRow swap={swap} userId={userId}>
      <span className="text-xs font-medium px-2.5 py-1 rounded-full bg-yellow-100 text-yellow-700">Pending</span>
      {isReceiver ? (
        <div className="flex gap-2">
          <button
            onClick={() => updateStatus.mutate({ id: swap.id, dto: { status: 'Accepted' } })}
            className="flex items-center gap-1 text-sm px-3 py-1.5 bg-green-600 text-white rounded-lg hover:bg-green-700"
          >
            <Check size={14} /> Accept
          </button>
          <button
            onClick={() => updateStatus.mutate({ id: swap.id, dto: { status: 'Rejected' } })}
            className="flex items-center gap-1 text-sm px-3 py-1.5 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            <X size={14} /> Reject
          </button>
        </div>
      ) : (
        <button
          onClick={() => updateStatus.mutate({ id: swap.id, dto: { status: 'Cancelled' } })}
          className="text-sm px-3 py-1.5 text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
        >
          Cancel
        </button>
      )}
    </SwapRow>
  );
}

/* ─── Accepted: Receiver proposes schedule, Requester confirms ─── */
function AcceptedSwapCard({ swap, userId }: { swap: Swap; userId: string }) {
  const isReceiver = swap.receiverId === userId;
  const isRequester = swap.requesterId === userId;
  const proposeSchedule = useProposeSchedule();
  const confirmSchedule = useConfirmSchedule();
  const requestScheduleChange = useRequestScheduleChange();
  const [showReport, setShowReport] = useState(false);
  const [showChangeRequest, setShowChangeRequest] = useState(false);
  const [changeNote, setChangeNote] = useState('');
  const [changeTime, setChangeTime] = useState('');
  const [showRescheduleForm, setShowRescheduleForm] = useState(true);

  const [requestedSingleStart, setRequestedSingleStart] = useState('');
  const [requestedStartDate, setRequestedStartDate] = useState('');
  const [requestedTimeOfDay, setRequestedTimeOfDay] = useState('');
  const [requestedWeekDays, setRequestedWeekDays] = useState<number[]>([]);

  const [offeredSingleStart, setOfferedSingleStart] = useState('');
  const [offeredStartDate, setOfferedStartDate] = useState('');
  const [offeredTimeOfDay, setOfferedTimeOfDay] = useState('');
  const [offeredWeekDays, setOfferedWeekDays] = useState<number[]>([]);

  const hasProposedSchedule = swap.schedules && swap.schedules.length > 0;
  const changeSchedule = swap.schedules?.find((schedule) => schedule.changeRequestNote) ?? null;
  const hasChangeRequest = !!changeSchedule;
  const isExchange = swap.lessonType === 'Exchange';
  const requiresOfferedSchedule = isExchange && swap.twoWayScheduleMode === 'Separate';
  const canRequestChange =
    isRequester
    && swap.lessonType === 'OneWay'
    && swap.requestedCadence === 'Single'
    && hasProposedSchedule
    && !hasChangeRequest;

  const toggleWeekDay = (current: number[], day: number) =>
    current.includes(day) ? current.filter((d) => d !== day) : [...current, day];

  const buildProposal = () => {
    const tracks = [] as any[];
    if (swap.requestedCadence === 'Single') {
      tracks.push({
        track: 'RequestedSkill',
        cadence: 'Single',
        sessionCount: swap.requestedSessionCount,
        singleSessionStart: requestedSingleStart ? new Date(requestedSingleStart).toISOString() : null,
      });
    } else {
      tracks.push({
        track: 'RequestedSkill',
        cadence: 'Weekly',
        sessionCount: swap.requestedSessionCount,
        startDate: requestedStartDate ? new Date(`${requestedStartDate}T00:00`).toISOString() : null,
        timeOfDayMinutes: requestedTimeOfDay ? timeToMinutes(requestedTimeOfDay) : null,
        weekDays: requestedWeekDays,
      });
    }

    if (requiresOfferedSchedule) {
      if (swap.offeredCadence === 'Single') {
        tracks.push({
          track: 'OfferedSkill',
          cadence: 'Single',
          sessionCount: swap.offeredSessionCount ?? 1,
          singleSessionStart: offeredSingleStart ? new Date(offeredSingleStart).toISOString() : null,
        });
      } else {
        tracks.push({
          track: 'OfferedSkill',
          cadence: 'Weekly',
          sessionCount: swap.offeredSessionCount ?? 1,
          startDate: offeredStartDate ? new Date(`${offeredStartDate}T00:00`).toISOString() : null,
          timeOfDayMinutes: offeredTimeOfDay ? timeToMinutes(offeredTimeOfDay) : null,
          weekDays: offeredWeekDays,
        });
      }
    }

    return { tracks };
  };

  const requestedReady = swap.requestedCadence === 'Single'
    ? !!requestedSingleStart
    : !!requestedStartDate && !!requestedTimeOfDay && requestedWeekDays.length > 0;

  const offeredReady = !requiresOfferedSchedule
    || (swap.offeredCadence === 'Single'
      ? !!offeredSingleStart
      : !!offeredStartDate && !!offeredTimeOfDay && offeredWeekDays.length > 0);

  const canPropose = requestedReady && offeredReady;

  useEffect(() => {
    if (hasChangeRequest) {
      setShowRescheduleForm(false);
    }
  }, [hasChangeRequest]);

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-3">
      <div className="flex justify-between items-start">
        <SwapHeader swap={swap} userId={userId} />
        <button
          onClick={() => setShowReport(true)}
          className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
          title="Report Issue"
        >
          <AlertTriangle size={16} />
        </button>
      </div>
      <span className="inline-block text-xs font-medium px-2.5 py-1 rounded-full bg-green-100 text-green-700">Accepted</span>

      {isReceiver && (!hasProposedSchedule || showRescheduleForm) && (
        <div className="bg-gray-50 rounded-lg p-3 space-y-4">
          <p className="text-sm font-medium text-gray-700 flex items-center gap-1">
            <Clock size={14} /> {hasChangeRequest ? 'Propose an updated schedule' : 'Propose the schedule'} (60-minute sessions)
          </p>

          <ScheduleForm
            title={`Requested skill (${swap.requestedCadence === 'Single' ? 'Single session' : 'Weekly recurring'})`}
            cadence={swap.requestedCadence}
            sessionCount={swap.requestedSessionCount}
            singleStart={requestedSingleStart}
            setSingleStart={setRequestedSingleStart}
            startDate={requestedStartDate}
            setStartDate={setRequestedStartDate}
            timeOfDay={requestedTimeOfDay}
            setTimeOfDay={setRequestedTimeOfDay}
            weekDays={requestedWeekDays}
            setWeekDays={setRequestedWeekDays}
            toggleWeekDay={toggleWeekDay}
          />

          {requiresOfferedSchedule ? (
            <ScheduleForm
              title={`Offered skill (${swap.offeredCadence === 'Single' ? 'Single session' : 'Weekly recurring'})`}
              cadence={swap.offeredCadence ?? 'Single'}
              sessionCount={swap.offeredSessionCount ?? 1}
              singleStart={offeredSingleStart}
              setSingleStart={setOfferedSingleStart}
              startDate={offeredStartDate}
              setStartDate={setOfferedStartDate}
              timeOfDay={offeredTimeOfDay}
              setTimeOfDay={setOfferedTimeOfDay}
              weekDays={offeredWeekDays}
              setWeekDays={setOfferedWeekDays}
              toggleWeekDay={toggleWeekDay}
            />
          ) : (
            isExchange && (
              <p className="text-sm text-gray-500">
                Consecutive mode: the offered skill will be scheduled right after the requested sessions.
              </p>
            )
          )}

          <button
            disabled={!canPropose || proposeSchedule.isPending}
            onClick={() => proposeSchedule.mutate({ id: swap.id, dto: buildProposal() })}
            className="px-4 py-1.5 text-sm bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
          >
            Propose schedule
          </button>
        </div>
      )}

      {hasProposedSchedule && (
        <div className="bg-gray-50 rounded-lg p-3 space-y-2">
          <p className="text-sm font-medium text-gray-700">Schedule proposal</p>
          {isReceiver && hasChangeRequest && (
            <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">
              <span className="font-semibold">Change request:</span>{' '}
              {changeSchedule?.changeRequestNote}
              {changeSchedule?.changeRequestTime && (
                <span className="ml-2 text-amber-800">
                  Suggested: {formatDateTime(changeSchedule.changeRequestTime)}
                </span>
              )}
            </div>
          )}
          <div className="space-y-1 text-sm text-gray-600">
            {swap.schedules.map((schedule) => (
              <div key={schedule.id}>
                <span className="font-medium">
                  {schedule.track === 'RequestedSkill' ? 'Requested' : 'Offered'}:</span>{' '}
                {schedule.cadence === 'Single'
                  ? `Single session at ${schedule.singleSessionStart ? formatDateTime(schedule.singleSessionStart) : 'TBD'}`
                  : `Weekly (${schedule.sessionCount} sessions) on ${schedule.weekDays ?? 'TBD'} at ${schedule.timeOfDayMinutes != null ? minutesToTime(schedule.timeOfDayMinutes) : 'TBD'}`}
                {schedule.changeRequestNote && (
                  <div className="mt-1 text-xs text-amber-700">
                    Requested change: {schedule.changeRequestNote}
                    {schedule.changeRequestTime && ` (Suggested: ${formatDateTime(schedule.changeRequestTime)})`}
                  </div>
                )}
              </div>
            ))}
          </div>
          {isRequester ? (
            <div className="flex flex-wrap gap-2">
              <button
                disabled={confirmSchedule.isPending}
                onClick={() => confirmSchedule.mutate(swap.id)}
                className="px-4 py-1.5 text-sm bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
              >
                Confirm schedule
              </button>
              {canRequestChange && (
                <button
                  type="button"
                  onClick={() => setShowChangeRequest((prev) => !prev)}
                  className="px-4 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-white"
                >
                  Request another date
                </button>
              )}
            </div>
          ) : hasChangeRequest && swap.requestedCadence === 'Single' ? (
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                disabled={!changeSchedule?.changeRequestTime || proposeSchedule.isPending}
                onClick={() =>
                  proposeSchedule.mutate({
                    id: swap.id,
                    dto: {
                      tracks: [
                        {
                          track: 'RequestedSkill',
                          cadence: 'Single',
                          sessionCount: swap.requestedSessionCount,
                          singleSessionStart: changeSchedule?.changeRequestTime ?? null,
                        },
                      ],
                    },
                  })
                }
                className="px-4 py-1.5 text-sm bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
              >
                Accept suggested time
              </button>
              <button
                type="button"
                onClick={() => setShowRescheduleForm(true)}
                className="px-4 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-white"
              >
                Suggest another time
              </button>
            </div>
          ) : (
            <p className="text-sm text-gray-500">Waiting for the other party to confirm.</p>
          )}

          {canRequestChange && showChangeRequest && (
            <div className="mt-3 space-y-2">
              <label className="text-xs text-gray-500">
                Why this time doesn't work (max 100 chars)
                <textarea
                  value={changeNote}
                  onChange={(e) => setChangeNote(e.target.value.slice(0, 100))}
                  rows={2}
                  maxLength={100}
                  className="block mt-1 w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 outline-none resize-none"
                />
              </label>
              <label className="text-xs text-gray-500">
                Suggested time
                <input
                  type="datetime-local"
                  value={changeTime}
                  onChange={(e) => setChangeTime(e.target.value)}
                  className="block mt-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 outline-none"
                />
              </label>
              <button
                type="button"
                disabled={!changeNote.trim() || !changeTime || requestScheduleChange.isPending}
                onClick={() =>
                  requestScheduleChange.mutate({
                    id: swap.id,
                    dto: {
                      track: 'RequestedSkill',
                      note: changeNote.trim(),
                      suggestedTime: new Date(changeTime).toISOString(),
                    }
                  }, {
                    onSuccess: () => {
                      setShowChangeRequest(false);
                      setChangeNote('');
                      setChangeTime('');
                    }
                  })
                }
                className="px-4 py-1.5 text-sm bg-amber-600 text-white rounded-lg hover:bg-amber-700 disabled:opacity-50"
              >
                Send request
              </button>
            </div>
          )}
        </div>
      )}
      <ReportModal isOpen={showReport} onClose={() => setShowReport(false)} swapRequestId={swap.id} />
    </div>
  );
}

/* ─── Scheduled: Validate per session ─── */
function ScheduledSwapCard({ swap, userId }: { swap: Swap; userId: string }) {
  const validateSession = useValidateSession();
  const invalidateSession = useInvalidateSession();
  const isRequester = swap.requesterId === userId;
  const [showReport, setShowReport] = useState(false);

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-3">
      <div className="flex justify-between items-start">
        <SwapHeader swap={swap} userId={userId} />
        <button
          onClick={() => setShowReport(true)}
          className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
          title="Report Issue"
        >
          <AlertTriangle size={16} />
        </button>
      </div>
      <div className="flex flex-wrap items-center gap-2">
        <span className="text-xs font-medium px-2.5 py-1 rounded-full bg-purple-100 text-purple-700">
          Scheduled — {swap.sessions.length} sessions
        </span>
      </div>

      <div className="bg-gray-50 rounded-lg p-3 space-y-3">
        <p className="text-sm text-gray-700 font-medium">Session validation</p>
        <div className="space-y-2">
          {swap.sessions.map((session) => {
            const hasValidated = isRequester ? session.requesterValidated : session.receiverValidated;
            const otherValidated = isRequester ? session.receiverValidated : session.requesterValidated;
            return (
              <div key={session.id} className="border border-gray-200 bg-white rounded-lg p-3">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div className="text-sm text-gray-700">
                    <span className="font-medium">{session.track === 'RequestedSkill' ? 'Requested' : 'Offered'} session {session.sessionOrder}</span>
                    <span className="text-gray-500"> · {formatDateTime(session.startTime)}</span>
                  </div>
                  <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${session.status === 'Completed' ? 'bg-green-100 text-green-700' : session.status === 'Invalidated' ? 'bg-red-100 text-red-700' : 'bg-gray-100 text-gray-600'}`}>
                    {session.status}
                  </span>
                </div>

                {session.status === 'Scheduled' && (
                  <div className="mt-2 flex flex-wrap gap-2 items-center">
                    <span className={`text-xs ${hasValidated ? 'text-green-600' : 'text-gray-500'}`}>
                      {hasValidated ? 'You validated' : 'You have not validated'}
                    </span>
                    <span className={`text-xs ${otherValidated ? 'text-green-600' : 'text-gray-500'}`}>
                      {otherValidated ? 'Other party validated' : 'Other party pending'}
                    </span>
                  </div>
                )}

                {session.status === 'Scheduled' && !hasValidated && (
                  <div className="mt-2 flex gap-2">
                    <button
                      onClick={() => validateSession.mutate({ id: swap.id, sessionId: session.id })}
                      disabled={validateSession.isPending}
                      className="flex items-center gap-1 text-sm px-4 py-1.5 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
                    >
                      <Check size={14} /> Validate
                    </button>
                    <button
                      onClick={() => invalidateSession.mutate({ id: swap.id, sessionId: session.id })}
                      disabled={invalidateSession.isPending}
                      className="flex items-center gap-1 text-sm px-4 py-1.5 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
                    >
                      <X size={14} /> Invalidate
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
      <ReportModal isOpen={showReport} onClose={() => setShowReport(false)} swapRequestId={swap.id} />
    </div>
  );
}

/* ─── Completed: Leave Review ─── */
function CompletedSwapCard({ swap, userId }: { swap: Swap; userId: string }) {
  const [showReview, setShowReview] = useState(false);
  const [showReport, setShowReport] = useState(false);
  const { data: hasReviewed } = useHasReviewed(swap.id);
  const otherName = swap.requesterId === userId ? swap.receiverName : swap.requesterName;
  const otherUserId = swap.requesterId === userId ? swap.receiverId : swap.requesterId;

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4 flex justify-between items-center">
      <div>
        <SwapSummary swap={swap} userId={userId} showLink />
      </div>
      <div className="flex items-center gap-3">
        <span className="text-xs font-medium px-2.5 py-1 rounded-full bg-blue-100 text-blue-700">Completed</span>
        {hasReviewed ? (
          <span className="flex items-center gap-1 text-xs text-green-600 font-medium">
            <Star size={14} fill="currentColor" /> Reviewed
          </span>
        ) : (
          <button
            onClick={() => setShowReview(true)}
            className="flex items-center gap-1 text-sm px-3 py-1.5 bg-yellow-500 text-white rounded-lg hover:bg-yellow-600"
          >
            <Star size={14} /> Leave Review
          </button>
        )}
        <button
          onClick={() => setShowReport(true)}
          className="flex items-center gap-1 text-sm px-3 py-1.5 text-gray-700 border border-gray-300 bg-white hover:bg-gray-50 rounded-lg"
          title="Report Issue"
        >
          <AlertTriangle size={14} className="text-red-500" /> Report
        </button>
      </div>
      {showReview && (
        <ReviewModal
          swapRequestId={swap.id}
          otherUserName={otherName}
          onClose={() => setShowReview(false)}
        />
      )}
      <ReportModal isOpen={showReport} onClose={() => setShowReport(false)} swapRequestId={swap.id} />
    </div>
  );
}

/* ─── Shared components ─── */
function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="mb-8">
      <h2 className="text-lg font-semibold text-gray-900 mb-3">{title}</h2>
      <div className="space-y-3">{children}</div>
    </div>
  );
}

function SwapHeader({ swap, userId }: { swap: Swap; userId: string }) {
  return <SwapSummary swap={swap} userId={userId} showLink />;
}

const weekDayOptions = [
  { label: 'Sun', value: 0 },
  { label: 'Mon', value: 1 },
  { label: 'Tue', value: 2 },
  { label: 'Wed', value: 3 },
  { label: 'Thu', value: 4 },
  { label: 'Fri', value: 5 },
  { label: 'Sat', value: 6 },
];

function ScheduleForm({
  title,
  cadence,
  sessionCount,
  singleStart,
  setSingleStart,
  startDate,
  setStartDate,
  timeOfDay,
  setTimeOfDay,
  weekDays,
  setWeekDays,
  toggleWeekDay,
}: {
  title: string;
  cadence: 'Single' | 'Weekly';
  sessionCount: number;
  singleStart: string;
  setSingleStart: (value: string) => void;
  startDate: string;
  setStartDate: (value: string) => void;
  timeOfDay: string;
  setTimeOfDay: (value: string) => void;
  weekDays: number[];
  setWeekDays: (value: number[]) => void;
  toggleWeekDay: (current: number[], day: number) => number[];
}) {
  return (
    <div className="border border-gray-200 rounded-lg p-3 bg-white space-y-2">
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-gray-700">{title}</p>
        <span className="text-xs text-gray-500">{sessionCount} sessions</span>
      </div>
      {cadence === 'Single' ? (
        <label className="text-xs text-gray-500">
          Session time
          <input
            type="datetime-local"
            value={singleStart}
            onChange={(e) => setSingleStart(e.target.value)}
            className="block mt-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 outline-none"
          />
        </label>
      ) : (
        <div className="space-y-2">
          <div className="flex gap-2 flex-wrap">
            <label className="text-xs text-gray-500">
              Start date
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="block mt-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 outline-none"
              />
            </label>
            <label className="text-xs text-gray-500">
              Time of day
              <input
                type="time"
                value={timeOfDay}
                onChange={(e) => setTimeOfDay(e.target.value)}
                className="block mt-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 outline-none"
              />
            </label>
          </div>
          <div className="flex flex-wrap gap-2">
            {weekDayOptions.map((day) => (
              <button
                type="button"
                key={day.value}
                onClick={() => setWeekDays(toggleWeekDay(weekDays, day.value))}
                className={`px-2 py-1 text-xs rounded-full border ${weekDays.includes(day.value) ? 'bg-primary-600 text-white border-primary-600' : 'border-gray-300 text-gray-600'}`}
              >
                {day.label}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function SwapRow({ swap, userId, children }: { swap: Swap; userId: string; children?: React.ReactNode }) {
  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4 flex justify-between items-center">
      <SwapSummary swap={swap} userId={userId} />
      <div className="flex items-center gap-3">
        {children}
      </div>
    </div>
  );
}

function SwapSummary({ swap, userId, showLink }: { swap: Swap; userId: string; showLink?: boolean }) {
  const lessonType = swap.lessonType ?? 'OneWay';
  const isExchange = lessonType === 'Exchange';
  const teacherName = swap.teacherName ?? swap.receiverName;
  const learnerName = swap.learnerName ?? swap.requesterName;
  const otherName = swap.requesterId === userId ? swap.receiverName : swap.requesterName;
  const otherUserId = swap.requesterId === userId ? swap.receiverId : swap.requesterId;

  return (
    <div>
      <div className="flex flex-wrap items-center gap-2">
        <span className={`text-[11px] font-semibold px-2 py-0.5 rounded-full ${
          isExchange ? 'bg-indigo-100 text-indigo-700' : 'bg-emerald-100 text-emerald-700'
        }`}>
          {isExchange ? 'Skill Exchange' : 'One-Way Lesson'}
        </span>
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
      </div>
      <p className="text-sm text-gray-500">
        {isExchange ? 'Teaches each other' : `Lesson: ${swap.requestedSkillTitle}`}{' '}
        with{' '}
        {showLink ? (
          <Link to={`/users/${otherUserId}`} className="text-primary-600 hover:text-primary-800 font-medium">
            {otherName}
          </Link>
        ) : (
          otherName
        )}
      </p>
    </div>
  );
}

function formatDateTime(iso: string) {
  return new Date(iso).toLocaleString([], {
    month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
  });
}

function timeToMinutes(value: string) {
  const [hours, minutes] = value.split(':').map(Number);
  return hours * 60 + minutes;
}

function minutesToTime(value: number) {
  const hours = Math.floor(value / 60).toString().padStart(2, '0');
  const minutes = (value % 60).toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}
