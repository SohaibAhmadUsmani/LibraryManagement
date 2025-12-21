namespace LibraryManagement.Models
{
    public class DashboardModel
    {
        public int TotalStudents { get; set; }
        public int TotalBooks { get; set; }
        public int TotalLibrarians { get; set; }
        public int TotalBorrowings { get; set; }
        public int PendingApprovals { get; set; }
        public int OverdueBorrowings { get; set; }
        public List<BorrowRequestModel> ActiveBorrowings { get; set; } = new List<BorrowRequestModel>();
    }
}