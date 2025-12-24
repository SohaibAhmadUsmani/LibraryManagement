using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models // Ensure namespace is LibraryManagement.Models
{
    [Table("Borrowings")]
    public class BorrowingModel
    {
        [Key]
        public int RequestId { get; set; }

        public int StudentId { get; set; }
        public int BookId { get; set; }

        // FIX: Initialize with string.Empty to stop warnings
        public string BookName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public string Status { get; set; } = "Pending";
    }
}