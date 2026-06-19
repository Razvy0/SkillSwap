import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { skillService, SkillQueryParams, CreateSkillDto } from '@/services/skillService';

export function useSkills(params?: SkillQueryParams) {
  return useQuery({
    queryKey: ['skills', params],
    queryFn: () => skillService.getSkills(params).then((r) => r.data),
  });
}

export function useMySkills() {
  return useQuery({
    queryKey: ['skills', 'my'],
    queryFn: () => skillService.getMySkills().then((r) => r.data),
  });
}

export function useUserSkills(userId: string) {
  return useQuery({
    queryKey: ['skills', 'user', userId],
    queryFn: () => skillService.getUserSkills(userId).then((r) => r.data),
    enabled: !!userId,
  });
}

export function useCreateSkill() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateSkillDto) => skillService.createSkill(dto),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['skills'] });
      qc.invalidateQueries({ queryKey: ['me'] });
    },
  });
}

export function useDeleteSkill() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => skillService.deleteSkill(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['skills'] });
      qc.invalidateQueries({ queryKey: ['me'] });
    },
  });
}
