using RChat.Data;
using RChat.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RChat.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(string userName, string content, string roomName)
        {
            var message = new Message
            {
                UserName = userName,
                Content = content,
                RoomName = roomName,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetRecentMessagesAsync(string roomName, int count = 50)
        {
            return await _context.Messages
                .Where(m => m.RoomName == roomName)
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .OrderBy(m => m.Timestamp) // Re-order for display
                .ToListAsync();
        }

        public async Task<List<string>> GetRoomListAsync()
        {
            return await _context.Messages
                .Select(m => m.RoomName)
                .Distinct()
                .ToListAsync();
        }
    }
}