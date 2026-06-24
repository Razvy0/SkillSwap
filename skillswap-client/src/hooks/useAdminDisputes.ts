import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/services/api'; // Adjust based on your actual api instance setup

export enum DisputeAction {
    BanUser = 0,
    DeleteSwap = 1,
    Dismiss = 2
}

export interface ResolveDisputePayload {
    disputeId: number;
    action: DisputeAction;
    adminNotes: string;
}

export function useAdminDisputes() {
    return useQuery({
        queryKey: ['adminDisputes'],
        queryFn: async () => {
            const response = await api.get('/Disputes/all');
            return response.data; // Assuming axios. Adjust if using fetch.
        }
    });
}

export function useResolveDispute() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ disputeId, action, adminNotes }: ResolveDisputePayload) => {
            const response = await api.post(`/Disputes/${disputeId}/resolve`, {
                action,
                adminNotes
            });
            return response.data;
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['adminDisputes'] });
        }
    });
}