using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using RChat.Services;
using System.Security.Claims;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace RChat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IUserTrackingService _userTrackingService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, IUserTrackingService userTrackingService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _userTrackingService = userTrackingService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User {UserName} connected with connection ID {ConnectionId}",
                Context.User.Identity.Name, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("User {UserName} disconnected with connection ID {ConnectionId}",
                Context.User.Identity.Name, Context.ConnectionId);

            if (exception != null)
            {
                _logger.LogError(exception, "User {UserName} disconnected due to an error", Context.User.Identity.Name);
            }

            var userRooms = await _userTrackingService.GetAllRooms();
            foreach (var roomName in userRooms)
            {
                var usersInRoom = await _userTrackingService.GetUsersInRoom(roomName);
                if (usersInRoom.Contains(Context.User.Identity.Name))
                {
                    await Clients.Group(roomName).SendAsync("UserLeft", Context.User.Identity.Name, roomName);
                    await _userTrackingService.UserLeftRoom(Context.UserIdentifier, roomName);

                    var userCount = await _userTrackingService.GetUserCountInRoom(roomName);
                    await Clients.Group(roomName).SendAsync("UpdateOnlineCount", userCount);
                    break;
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomName))
                {
                    throw new ArgumentException("Room name cannot be empty");
                }

                _logger.LogInformation("User {UserName} attempting to join room {RoomName}",
                    Context.User.Identity.Name, roomName);

                // Get user's current room if any
                var currentUserRooms = await _userTrackingService.GetAllRooms();
                string previousRoom = null;

                foreach (var room in currentUserRooms)
                {
                    var usersInRoom = await _userTrackingService.GetUsersInRoom(room);
                    if (usersInRoom.Contains(Context.User.Identity.Name))
                    {
                        previousRoom = room;
                        break;
                    }
                }

                // Remove user from previous room if any
                if (!string.IsNullOrEmpty(previousRoom))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousRoom);
                    await Clients.Group(previousRoom).SendAsync("UserLeft", Context.User.Identity.Name, previousRoom);
                    await _userTrackingService.UserLeftRoom(Context.UserIdentifier, previousRoom);

                    var previousRoomCount = await _userTrackingService.GetUserCountInRoom(previousRoom);
                    await Clients.Group(previousRoom).SendAsync("UpdateOnlineCount", previousRoomCount);
                }

                // Add user to new room
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                await _userTrackingService.UserJoinedRoom(Context.UserIdentifier, Context.User.Identity.Name, roomName);

                // Load chat history for this room
                var messages = await _messageService.GetRecentMessagesAsync(roomName);
                await Clients.Caller.SendAsync("LoadMessageHistory", messages);

                // Get current users in room and send to caller
                var currentUsers = await _userTrackingService.GetUsersInRoom(roomName);
                await Clients.Caller.SendAsync("UpdateOnlineUsers", currentUsers);

                // Notify the group about new user
                await Clients.OthersInGroup(roomName).SendAsync("UserJoined", Context.User.Identity.Name, roomName);

                // Update online count for everyone in the room
                var userCount = await _userTrackingService.GetUserCountInRoom(roomName);
                await Clients.Group(roomName).SendAsync("UpdateOnlineCount", userCount);

                // Send confirmation to the user
                await Clients.Caller.SendAsync("JoinedRoom", roomName);

                _logger.LogInformation("User {UserName} successfully joined room {RoomName}",
                    Context.User.Identity.Name, roomName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room {RoomName} for user {UserName}",
                    roomName, Context.User.Identity.Name);
                await Clients.Caller.SendAsync("Error", $"Failed to join room: {ex.Message}");
            }
        }

        public async Task SendMessage(string roomName, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "Message cannot be empty");
                    return;
                }

                if (message.Length > 1000)
                {
                    await Clients.Caller.SendAsync("Error", "Message is too long. Maximum 1000 characters allowed.");
                    return;
                }

                var user = Context.User.Identity.Name;
                var timestamp = DateTime.UtcNow;

                _logger.LogInformation("User {UserName} sending message to room {RoomName}", user, roomName);

                // Save message to database
                await _messageService.SaveMessageAsync(user, message, roomName);

                // Broadcast to room
                await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message, timestamp);

                _logger.LogDebug("Message from {UserName} delivered to room {RoomName}", user, roomName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to room {RoomName} for user {UserName}",
                    roomName, Context.User.Identity.Name);
                await Clients.Caller.SendAsync("Error", "Failed to send message. Please try again.");
            }
        }

        public async Task SendTypingNotification(string roomName, bool isTyping)
        {
            try
            {
                var user = Context.User.Identity.Name;
                await Clients.OthersInGroup(roomName).SendAsync("UserTyping", user, isTyping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing notification for user {UserName}", Context.User.Identity.Name);
            }
        }
    }
}