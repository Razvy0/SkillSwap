import api from './api';

export type LessonType = 'OneWay' | 'Exchange';
export type LessonCadence = 'Single' | 'Weekly';
export type TwoWayScheduleMode = 'Consecutive' | 'Separate';
export type SwapTrack = 'RequestedSkill' | 'OfferedSkill';
export type SwapScheduleStatus = 'Proposed' | 'Confirmed';
export type SwapSessionStatus = 'Proposed' | 'Scheduled' | 'Completed' | 'Invalidated';

export interface Swap {
  id: number;
  requesterId: string;
  requesterName: string;
  receiverId: string;
  receiverName: string;
  lessonType?: LessonType;
  requestedCadence: LessonCadence;
  offeredCadence?: LessonCadence | null;
  twoWayScheduleMode: TwoWayScheduleMode;
  teacherId?: string | null;
  teacherName?: string | null;
  learnerId?: string | null;
  learnerName?: string | null;
  offeredSkillTitle: string;
  requestedSkillTitle: string;
  requestedSessionCount: number;
  offeredSessionCount?: number | null;
  status: string;
  scheduledDate?: string;
  timeSlotStart?: string;
  timeSlotEnd?: string;
  requesterValidated: boolean;
  receiverValidated: boolean;
  schedules: SwapSchedule[];
  sessions: SwapSession[];
  createdAt: string;
  updatedAt?: string;
}

export interface SwapSchedule {
  id: number;
  track: SwapTrack;
  cadence: LessonCadence;
  sessionCount: number;
  weekDays?: string | null;
  timeOfDayMinutes?: number | null;
  singleSessionStart?: string | null;
  startDate?: string | null;
  changeRequestNote?: string | null;
  changeRequestTime?: string | null;
  changeRequestedById?: string | null;
  changeRequestedAt?: string | null;
  status: SwapScheduleStatus;
  proposedById: string;
  proposedAt: string;
}

export interface SwapSession {
  id: number;
  track: SwapTrack;
  sessionOrder: number;
  startTime: string;
  endTime: string;
  status: SwapSessionStatus;
  requesterValidated: boolean;
  receiverValidated: boolean;
}

export interface CreateSwapDto {
  offeredSkillId?: number | null;
  requestedSkillId: number;
  lessonType: LessonType;
  requestedCadence: LessonCadence;
  offeredCadence?: LessonCadence | null;
  twoWayScheduleMode: TwoWayScheduleMode;
}

export interface UpdateSwapStatusDto {
  status: string;
}

export interface ProposeTimeSlotDto {
  timeSlotStart: string;
  timeSlotEnd: string;
}

export interface PickTimeDto {
  scheduledDate: string;
}

export interface ScheduleTrackProposalDto {
  track: SwapTrack;
  cadence: LessonCadence;
  sessionCount: number;
  singleSessionStart?: string | null;
  startDate?: string | null;
  weekDays?: number[] | null;
  timeOfDayMinutes?: number | null;
}

export interface ProposeScheduleDto {
  tracks: ScheduleTrackProposalDto[];
}

export interface RequestScheduleChangeDto {
  track: SwapTrack;
  note: string;
  suggestedTime: string;
}

export const swapService = {
  getSwaps: () => api.get<Swap[]>('/swaps'),
  createSwap: (dto: CreateSwapDto) => api.post<Swap>('/swaps', dto),
  updateStatus: (id: number, dto: UpdateSwapStatusDto) => api.put<Swap>(`/swaps/${id}/status`, dto),
  proposeTimeSlot: (id: number, dto: ProposeTimeSlotDto) => api.put<Swap>(`/swaps/${id}/timeslot`, dto),
  pickTime: (id: number, dto: PickTimeDto) => api.put<Swap>(`/swaps/${id}/pick-time`, dto),
  proposeSchedule: (id: number, dto: ProposeScheduleDto) => api.put<Swap>(`/swaps/${id}/schedule/propose`, dto),
  confirmSchedule: (id: number) => api.put<Swap>(`/swaps/${id}/schedule/confirm`),
  requestScheduleChange: (id: number, dto: RequestScheduleChangeDto) =>
    api.put<Swap>(`/swaps/${id}/schedule/request-change`, dto),
  validateSession: (id: number, sessionId: number) => api.put<Swap>(`/swaps/${id}/sessions/${sessionId}/validate`),
  invalidateSession: (id: number, sessionId: number) => api.put<Swap>(`/swaps/${id}/sessions/${sessionId}/invalidate`),
  validate: (id: number) => api.put<Swap>(`/swaps/${id}/validate`),
  invalidate: (id: number) => api.put<Swap>(`/swaps/${id}/invalidate`),
};
