using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("Books")]
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public string? Publisher { get; set; }

        // --- TRANSLATOR ---
        [NotMapped] public string BookName { get => Title; set => Title = value; }
    }
}