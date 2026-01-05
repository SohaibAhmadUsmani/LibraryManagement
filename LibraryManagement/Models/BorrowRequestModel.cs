using System;

namespace LibraryManagement.Models
{
    public class BorrowRequestModel
    {
        public int RequestId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string BookName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; } // Added this back for you
        public string Status { get; set; } = string.Empty;
    }
}