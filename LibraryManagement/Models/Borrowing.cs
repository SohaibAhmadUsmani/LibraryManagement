using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Needed for [Table]

namespace LibraryManagement.Models
{
    [Table("Borrowing")] // Forces EF to use the Singular table name
    public class Borrowing
    {
        [Key]
        public int RequestId { get; set; }  // MATCHES DATABASE: RequestId

    
        public string StudentName { get; set; }
         public string BookName { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public string Status { get; set; }
        public int? FineAmount { get; set; } = 0;
    }
}