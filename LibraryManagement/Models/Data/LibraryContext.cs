using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models; // Make sure this is Models, not Models.Entities

namespace LibraryManagement.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<BookModel> Books { get; set; }
        // public DbSet<BorrowingModel> Borrowings { get; set; } 
    }
}