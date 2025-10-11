using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RChat.Services
{
    public class UserTrackingService : IUserTrackingService
    {
        private readonly ConcurrentDictionary<string, UserRoomInfo> _userRooms = new ConcurrentDictionary<string, UserRoomInfo>();

        private class UserRoomInfo
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string RoomName { get; set; }
        }

        public Task UserJoinedRoom(string userId, string userName, string roomName)
        {
            _userRooms[userId] = new UserRoomInfo
            {
                UserId = userId,
                UserName = userName,
                RoomName = roomName
            };
            return Task.CompletedTask;
        }

        public Task UserLeftRoom(string userId, string roomName)
        {
            _userRooms.TryRemove(userId, out _);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetUsersInRoom(string roomName)
        {
            var users = _userRooms.Values
                .Where(x => x.RoomName == roomName)
                .Select(x => x.UserName)
                .OrderBy(x => x)
                .ToList();
            return Task.FromResult(users);
        }

        public Task<int> GetUserCountInRoom(string roomName)
        {
            var count = _userRooms.Values.Count(x => x.RoomName == roomName);
            return Task.FromResult(count);
        }

        public Task<List<string>> GetAllRooms()
        {
            var rooms = _userRooms.Values
                .Select(x => x.RoomName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            return Task.FromResult(rooms);
        }
    }
}