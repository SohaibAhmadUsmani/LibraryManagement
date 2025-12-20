namespace LibraryManagement.Models
{
    public class StudentDashboardModel
    {
        public string StudentName { get; set; } = string.Empty;
        public int MyBorrowedBooks { get; set; }
        public int OverdueBooks { get; set; }
        public int AvailableBooks { get; set; }
        public List<BorrowingModel> MyCurrentBorrowings { get; set; } = new();
        public List<BookModel> RecentBooks { get; set; } = new();
    }
}
