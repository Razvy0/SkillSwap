import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { messageService, SendMessageDto } from '@/services/messageService';

export function useConversations() {
    return useQuery({
        queryKey: ['conversations'],
        queryFn: () => messageService.getConversations().then((r) => r.data),
    });
}

export function useConversation(otherUserId: string) {
    return useQuery({
        queryKey: ['conversation', otherUserId],
        queryFn: () => messageService.getConversation(otherUserId).then((r) => r.data),
        enabled: !!otherUserId,
        refetchInterval: 5000,
    });
}

export function useSendMessage() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (dto: SendMessageDto) => messageService.sendMessage(dto),
        onSuccess: (_data, variables) => {
            qc.invalidateQueries({ queryKey: ['conversation', variables.receiverId] });
            qc.invalidateQueries({ queryKey: ['conversations'] });
        },
    });
}

export function useMarkAsRead() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (senderId: string) => messageService.markAsRead(senderId),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['conversations'] });
            qc.invalidateQueries({ queryKey: ['unreadCount'] });
        },
    });
}

export function useUnreadCount() {
    return useQuery({
        queryKey: ['unreadCount'],
        queryFn: () => messageService.getUnreadCount().then((r) => r.data.count),
        refetchInterval: 10000,
    });
}
