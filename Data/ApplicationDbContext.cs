using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RChat.Models;
using Microsoft.AspNetCore.Identity;

namespace RChat.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatRoom> ChatRooms { get; set; }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the Message entity for PostgreSQL
            builder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");

                entity.HasKey(m => m.Id);

                entity.Property(m => m.UserName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(m => m.Content)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(m => m.RoomName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(m => m.Timestamp)
                    .IsRequired();

                entity.HasIndex(m => m.RoomName);
                entity.HasIndex(m => m.Timestamp);
            });
        }
    }
}