import { useState, useRef, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useConversations, useConversation, useSendMessage, useMarkAsRead } from '@/hooks/useMessages';
import { useAuthStore } from '@/stores/authStore';
import { MessageSquare, Send, ArrowLeft } from 'lucide-react';

export default function MessagesPage() {
    const [searchParams] = useSearchParams();
    const [selectedUserId, setSelectedUserId] = useState<string | null>(
        searchParams.get('user')
    );

    return (
        <div className="h-[calc(100vh-4rem)]">
            <h1 className="text-2xl font-bold text-gray-900 mb-6">Messages</h1>
            <div className="flex h-[calc(100%-3.5rem)] bg-white border border-gray-200 rounded-xl overflow-hidden">
                <ConversationList
                    selectedUserId={selectedUserId}
                    onSelect={setSelectedUserId}
                />
                {selectedUserId ? (
                    <ChatPane
                        otherUserId={selectedUserId}
                        onBack={() => setSelectedUserId(null)}
                    />
                ) : (
                    <div className="flex-1 flex items-center justify-center text-gray-400">
                        <div className="text-center">
                            <MessageSquare size={48} className="mx-auto mb-3 opacity-50" />
                            <p>Select a conversation to start chatting</p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}

function ConversationList({
    selectedUserId,
    onSelect,
}: {
    selectedUserId: string | null;
    onSelect: (id: string) => void;
}) {
    const { data: conversations, isLoading } = useConversations();

    return (
        <div className="w-80 border-r border-gray-200 flex flex-col">
            <div className="p-4 border-b border-gray-200">
                <h2 className="font-semibold text-gray-900">Conversations</h2>
            </div>
            <div className="flex-1 overflow-y-auto">
                {isLoading ? (
                    <p className="p-4 text-sm text-gray-500">Loading...</p>
                ) : conversations && conversations.length > 0 ? (
                    conversations.map((c) => (
                        <button
                            key={c.userId}
                            onClick={() => onSelect(c.userId)}
                            className={`w-full text-left p-4 border-b border-gray-100 hover:bg-gray-50 transition-colors ${selectedUserId === c.userId ? 'bg-primary-50' : ''
                                }`}
                        >
                            <div className="flex justify-between items-start">
                                <div className="min-w-0 flex-1">
                                    <p className="font-medium text-gray-900 truncate">{c.fullName}</p>
                                    {c.lastMessage && (
                                        <p className="text-sm text-gray-500 truncate mt-0.5">{c.lastMessage}</p>
                                    )}
                                </div>
                                <div className="flex flex-col items-end ml-2">
                                    {c.lastMessageTimestamp && (
                                        <span className="text-xs text-gray-400">
                                            {formatTime(c.lastMessageTimestamp)}
                                        </span>
                                    )}
                                    {c.unreadCount > 0 && (
                                        <span className="mt-1 inline-flex items-center justify-center w-5 h-5 text-xs font-bold text-white bg-primary-600 rounded-full">
                                            {c.unreadCount}
                                        </span>
                                    )}
                                </div>
                            </div>
                        </button>
                    ))
                ) : (
                    <p className="p-4 text-sm text-gray-500">No conversations yet.</p>
                )}
            </div>
        </div>
    );
}

function ChatPane({ otherUserId, onBack }: { otherUserId: string; onBack: () => void }) {
    const userId = useAuthStore((s) => s.userId);
    const { data: messages } = useConversation(otherUserId);
    const sendMessage = useSendMessage();
    const markAsRead = useMarkAsRead();
    const [text, setText] = useState('');
    const bottomRef = useRef<HTMLDivElement>(null);
    const hasUnreadFromOther =
        messages?.some((msg) => msg.senderId === otherUserId && !msg.isRead) ?? false;

    // Mark as read when opening or receiving new messages in this conversation
    useEffect(() => {
        if (!otherUserId || !hasUnreadFromOther || markAsRead.isPending) return;
        markAsRead.mutate(otherUserId);
    }, [otherUserId, hasUnreadFromOther, markAsRead]);

    // Scroll to bottom on new messages
    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    const otherName = messages?.[0]
        ? messages[0].senderId === otherUserId
            ? messages[0].senderName
            : messages[0].receiverName
        : 'Chat';

    const handleSend = (e: React.FormEvent) => {
        e.preventDefault();
        if (!text.trim()) return;
        sendMessage.mutate({ receiverId: otherUserId, content: text.trim() });
        setText('');
    };

    return (
        <div className="flex-1 flex flex-col">
            {/* Header */}
            <div className="p-4 border-b border-gray-200 flex items-center gap-3">
                <button onClick={onBack} className="md:hidden text-gray-500 hover:text-gray-700">
                    <ArrowLeft size={20} />
                </button>
                <Link to={`/users/${otherUserId}`} className="font-semibold text-gray-900 hover:text-primary-600">
                    {otherName}
                </Link>
            </div>

            {/* Messages */}
            <div className="flex-1 overflow-y-auto p-4 space-y-3">
                {messages?.map((msg) => {
                    const isMine = msg.senderId === userId;
                    return (
                        <div key={msg.id} className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}>
                            <div
                                className={`max-w-[70%] px-4 py-2 rounded-2xl text-sm ${isMine
                                    ? 'bg-primary-600 text-white rounded-br-md'
                                    : 'bg-gray-100 text-gray-900 rounded-bl-md'
                                    }`}
                            >
                                <p>{msg.content}</p>
                                <p className={`text-xs mt-1 ${isMine ? 'text-primary-200' : 'text-gray-400'}`}>
                                    {formatTime(msg.timestamp)}
                                </p>
                            </div>
                        </div>
                    );
                })}
                <div ref={bottomRef} />
            </div>

            {/* Input */}
            <form onSubmit={handleSend} className="p-4 border-t border-gray-200 flex gap-2">
                <input
                    type="text"
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    placeholder="Type a message..."
                    className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none"
                />
                <button
                    type="submit"
                    disabled={!text.trim() || sendMessage.isPending}
                    className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 transition-colors"
                >
                    <Send size={18} />
                </button>
            </form>
        </div>
    );
}

function formatTime(iso: string) {
    const d = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    if (diffDays === 0) return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return d.toLocaleDateString([], { weekday: 'short' });
    return d.toLocaleDateString([], { month: 'short', day: 'numeric' });
}
