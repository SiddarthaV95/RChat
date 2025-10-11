using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RChat.Models;

namespace RChat.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the Message entity
            builder.Entity<Message>(entity =>
            {
                entity.HasIndex(m => m.RoomName);
                entity.HasIndex(m => m.Timestamp);
                entity.Property(m => m.UserName).IsRequired().HasMaxLength(256);
                entity.Property(m => m.Content).IsRequired().HasMaxLength(1000);
                entity.Property(m => m.RoomName).IsRequired().HasMaxLength(100);
            });
        }
    }
}