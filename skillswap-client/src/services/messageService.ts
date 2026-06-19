import api from './api';

export interface MessageDto {
    id: number;
    senderId: string;
    senderName: string;
    receiverId: string;
    receiverName: string;
    content: string;
    timestamp: string;
    isRead: boolean;
}

export interface ConversationDto {
    userId: string;
    fullName: string;
    lastMessage?: string;
    lastMessageTimestamp?: string;
    unreadCount: number;
}

export interface SendMessageDto {
    receiverId: string;
    content: string;
}

export const messageService = {
    getConversations: () =>
        api.get<ConversationDto[]>('/messages/conversations'),
    getConversation: (otherUserId: string) =>
        api.get<MessageDto[]>(`/messages/${otherUserId}`),
    sendMessage: (dto: SendMessageDto) =>
        api.post<MessageDto>('/messages', dto),
    markAsRead: (senderId: string) =>
        api.put(`/messages/read/${senderId}`),
    getUnreadCount: () =>
        api.get<{ count: number }>('/messages/unread/count'),
};
