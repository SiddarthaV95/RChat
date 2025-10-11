using System.Collections.Generic;
using System.Threading.Tasks;

namespace RChat.Services
{
    public interface IUserTrackingService
    {
        Task UserJoinedRoom(string userId, string userName, string roomName);
        Task UserLeftRoom(string userId, string roomName);
        Task<List<string>> GetUsersInRoom(string roomName);
        Task<int> GetUserCountInRoom(string roomName);
        Task<List<string>> GetAllRooms();
    }
}