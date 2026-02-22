// Models/ChatRoom.cs
using System.ComponentModel.DataAnnotations;

namespace RChat.Models
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsProtected { get; set; } = false;

        public string? PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public static string NormalizeName(string name)
            => name.TrimStart().TrimEnd().ToLowerInvariant();
    }
}