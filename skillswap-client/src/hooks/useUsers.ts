import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService, UpdateProfileDto, UserSearchParams } from '@/services/userService';

export function useMe() {
    return useQuery({
        queryKey: ['me'],
        queryFn: () => userService.getMe().then((r) => r.data),
    });
}

export function useUserProfile(id: string) {
    return useQuery({
        queryKey: ['user', id],
        queryFn: () => userService.getProfile(id).then((r) => r.data),
        enabled: !!id,
    });
}

export function useUpdateProfile() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (dto: UpdateProfileDto) => userService.updateProfile(dto),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['me'] });
            qc.invalidateQueries({ queryKey: ['profile'] });
        },
    });
}

export function useSearchUsers(params?: UserSearchParams) {
    return useQuery({
        queryKey: ['users', 'search', params],
        queryFn: () => userService.searchUsers(params).then((r) => r.data),
        enabled: !!(params?.name || params?.skill),
    });
}
