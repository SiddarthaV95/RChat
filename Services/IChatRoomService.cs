// Services/IChatRoomService.cs
using RChat.Models;

namespace RChat.Services
{
    public interface IChatRoomService
    {
        Task<ChatRoom?> GetRoomAsync(string roomName);
        Task<ChatRoom> CreateOrGetRoomAsync(string roomName, string? password = null);
        Task<bool> VerifyPasswordAsync(string roomName, string password);
        Task<List<ChatRoom>> GetAllRoomsAsync();
        Task SeedDefaultRoomsAsync();
    }
}