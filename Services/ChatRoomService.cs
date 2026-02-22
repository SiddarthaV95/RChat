// Services/ChatRoomService.cs
using Microsoft.EntityFrameworkCore;
using RChat.Models;
using BCrypt.Net;
using RChat.Data;

namespace RChat.Services
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly ApplicationDbContext _context;

        public ChatRoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRoom?> GetRoomAsync(string roomName)
        {
            var normalized = ChatRoom.NormalizeName(roomName);
            return await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.Name == normalized);
        }

        public async Task<ChatRoom> CreateOrGetRoomAsync(string roomName, string? password = null)
        {
            var normalized = ChatRoom.NormalizeName(roomName);
            var existing = await GetRoomAsync(normalized);
            if (existing != null) return existing;

            var room = new ChatRoom
            {
                Name = normalized,
                IsProtected = !string.IsNullOrWhiteSpace(password),
                PasswordHash = !string.IsNullOrWhiteSpace(password)
                    ? BCrypt.Net.BCrypt.HashPassword(password)
                    : null
            };

            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<bool> VerifyPasswordAsync(string roomName, string password)
        {
            var room = await GetRoomAsync(roomName);
            if (room == null || !room.IsProtected) return true;
            if (string.IsNullOrWhiteSpace(password)) return false;

            return BCrypt.Net.BCrypt.Verify(password, room.PasswordHash);
        }

        public async Task<List<ChatRoom>> GetAllRoomsAsync()
        {
            return await _context.ChatRooms
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task SeedDefaultRoomsAsync()
        {
            var defaults = new[] { "General", "Technology", "Games", "Random" };
            foreach (var name in defaults)
            {
                await CreateOrGetRoomAsync(name);
            }
        }
    }
}