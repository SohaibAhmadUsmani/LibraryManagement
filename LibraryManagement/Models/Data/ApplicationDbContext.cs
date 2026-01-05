using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models; // We are now purely using Models

namespace LibraryManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrowing> Borrowing { get; set; } // Matches the class 'Borrowing'
        public DbSet<Announcement> Announcements { get; set; }
    }

}