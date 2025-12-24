using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models // Namespace fixed
{
    [Table("Books")]
    public class BookModel
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        public string BookName { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string Publisher { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        public string ISBN { get; set; } = string.Empty;
    }
}