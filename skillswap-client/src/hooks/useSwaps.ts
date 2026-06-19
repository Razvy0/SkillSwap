import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  swapService,
  CreateSwapDto,
  UpdateSwapStatusDto,
  ProposeTimeSlotDto,
  PickTimeDto,
  ProposeScheduleDto,
  RequestScheduleChangeDto,
} from '@/services/swapService';

export function useSwaps() {
  return useQuery({
    queryKey: ['swaps'],
    queryFn: () => swapService.getSwaps().then((r) => r.data),
  });
}

export function useCreateSwap() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateSwapDto) => swapService.createSwap(dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useUpdateSwapStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: UpdateSwapStatusDto }) =>
      swapService.updateStatus(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useProposeTimeSlot() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: ProposeTimeSlotDto }) =>
      swapService.proposeTimeSlot(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function usePickTime() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: PickTimeDto }) =>
      swapService.pickTime(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useProposeSchedule() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: ProposeScheduleDto }) =>
      swapService.proposeSchedule(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useConfirmSchedule() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => swapService.confirmSchedule(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useRequestScheduleChange() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: RequestScheduleChangeDto }) =>
      swapService.requestScheduleChange(id, dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useValidateSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, sessionId }: { id: number; sessionId: number }) =>
      swapService.validateSession(id, sessionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useInvalidateSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, sessionId }: { id: number; sessionId: number }) =>
      swapService.invalidateSession(id, sessionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useValidateSwap() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => swapService.validate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}

export function useInvalidateSwap() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => swapService.invalidate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['swaps'] }),
  });
}
