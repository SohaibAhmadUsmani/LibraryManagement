using LibraryManagement.Models.Entities;

namespace LibraryManagement.Models
{
    public class LibrarianDashboardModel
    {
        public string LibrarianName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalBooks { get; set; }
        public int TotalBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public List<BorrowingModel> RecentBorrowings { get; set; } = new();
        public List<StudentModel> RecentStudents { get; set; } = new();
    }
}
