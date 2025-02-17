using Microsoft.EntityFrameworkCore;
using ToDoBackend.Models;

namespace ToDoBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<User> Users { get; set; }= null!;
        public DbSet<Community> Communities { get; set; }= null!;
        public DbSet<ToDoBackend.Models.Task> Tasks { get; set; }= null!;
        public DbSet<UserCommunity> UserCommunities { get; set; }= null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserCommunity için birleşik anahtar tanımı
            modelBuilder.Entity<UserCommunity>()
                .HasKey(uc => new { uc.UserId, uc.CommunityId });

            // İlişkiler
            modelBuilder.Entity<UserCommunity>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCommunities)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserCommunity>()
                .HasOne(uc => uc.Community)
                .WithMany(c => c.UserCommunities)
                .HasForeignKey(uc => uc.CommunityId);

            // Communities tablosu
            modelBuilder.Entity<Community>().ToTable("Communities");
        }
    }
}
