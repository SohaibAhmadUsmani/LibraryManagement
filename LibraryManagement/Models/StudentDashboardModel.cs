using LibraryManagement.Models.Entities;

namespace LibraryManagement.Models
{
    public class StudentDashboardModel
    {
        public string StudentName { get; set; } = string.Empty;
        public int MyBorrowedBooks { get; set; }
        public int OverdueBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int PendingRequests { get; set; }
        public List<BookModel> RecentBooks { get; set; } = new List<BookModel>();
        public List<BorrowingModel> MyCurrentBorrowings { get; set; } = new List<BorrowingModel>();
        public List<BorrowRequestModel> MyRequests { get; set; } = new List<BorrowRequestModel>();
    }
}