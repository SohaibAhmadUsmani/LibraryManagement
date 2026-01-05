namespace LibraryManagement.Models
{
    public class LibrarianDashboardModel
    {
        public int TotalStudents { get; set; }
        public int TotalBooks { get; set; }
        public int TotalBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
    }
}