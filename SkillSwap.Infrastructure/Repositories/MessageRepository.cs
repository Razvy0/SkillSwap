using Microsoft.EntityFrameworkCore;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Infrastructure.Data;

namespace SkillSwap.Infrastructure.Repositories;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2)
        => await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2)
                     || (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId)
        => await _dbSet
            .Include(m => m.Sender)
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

    public async Task MarkAsReadAsync(string senderId, string receiverId)
    {
        var unread = await _dbSet
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ToListAsync();

        foreach (var msg in unread)
            msg.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<(AppUser Partner, Message LastMessage, int UnreadCount)>> GetConversationPartnersAsync(string userId)
    {
        var messages = await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .ToListAsync();

        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var partnerId = g.Key;
                var lastMessage = g.OrderByDescending(m => m.Timestamp).First();
                var partner = lastMessage.SenderId == partnerId ? lastMessage.Sender : lastMessage.Receiver;
                var unreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead);
                return (Partner: partner, LastMessage: lastMessage, UnreadCount: unreadCount);
            })
            .OrderByDescending(c => c.LastMessage.Timestamp)
            .ToList();

        return conversations;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
        => await _dbSet.CountAsync(m => m.ReceiverId == userId && !m.IsRead);
}
