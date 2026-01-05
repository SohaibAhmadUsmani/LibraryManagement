using LibraryManagement.Models;
using System.Collections.Generic;

namespace LibraryManagement.Models
{
    public class StudentDashboardModel
    {
     
        public int TotalBorrowed { get; set; }
        public int PendingRequests { get; set; }
        public int OverdueBooks { get; set; }


        public string StudentName { get; set; } = string.Empty;

  
        public List<Book> RecentBooks { get; set; } = new List<Book>();
        public List<Book> AvailableBooks { get; set; } = new List<Book>();

        public List<Borrowing> MyRequests { get; set; } = new List<Borrowing>();
        public List<Borrowing> MyBorrowedBooks { get; set; } = new List<Borrowing>();
    }
}