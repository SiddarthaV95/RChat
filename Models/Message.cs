using System;
using System.ComponentModel.DataAnnotations;

namespace RChat.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string RoomName { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}