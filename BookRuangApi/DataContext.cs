using Microsoft.EntityFrameworkCore;
using BookRuangApi.Models;

namespace BookRuangApi
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<RoomLoan> RoomLoans { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin (opsional)
            // Uncomment jika mau auto-create admin saat pertama kali run
            /*
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@bookruang.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Administrator",
                    Role = UserRoles.Admin,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            );
            */
        }
    }
}