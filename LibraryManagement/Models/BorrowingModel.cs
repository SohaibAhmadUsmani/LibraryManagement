namespace LibraryManagement.Models
{
    public class BorrowingModel
    {
        public int BorrowingId { get; set; }
        public string StudentName { get; set; }
        public string BookName { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ReturnedDate { get; set; }
    }
}
