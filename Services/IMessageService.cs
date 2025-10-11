using RChat.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RChat.Services
{
    public interface IMessageService
    {
        Task SaveMessageAsync(string userName, string content, string roomName);
        Task<List<Message>> GetRecentMessagesAsync(string roomName, int count = 50);
        Task<List<string>> GetRoomListAsync();
    }
}