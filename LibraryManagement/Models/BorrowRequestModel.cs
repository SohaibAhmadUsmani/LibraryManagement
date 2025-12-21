namespace LibraryManagement.Models
{
    public class BorrowRequestModel
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
    }
}