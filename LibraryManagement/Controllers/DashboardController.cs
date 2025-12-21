using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _config;

        public DashboardController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new DashboardModel
            {
                TotalStudents = 0,
                TotalBooks = 0,
                TotalLibrarians = 0,
                TotalBorrowings = 0,
                PendingApprovals = 0,
                OverdueBorrowings = 0,
                ActiveBorrowings = new List<BorrowRequestModel>()
            };

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                // Total Students
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Student' AND IsActive = 1", con);
                model.TotalStudents = (int)cmd1.ExecuteScalar();

                // Total Books
                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM Books", con);
                model.TotalBooks = (int)cmd2.ExecuteScalar();

                // Total Librarians
                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Librarian' AND Status = 'Approved' AND IsActive = 1", con);
                model.TotalLibrarians = (int)cmd3.ExecuteScalar();

                // Total Active Borrowings
                SqlCommand cmd4 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE Status = 'Approved'", con);
                model.TotalBorrowings = (int)cmd4.ExecuteScalar();

                // Pending User Approvals (Librarians waiting for approval)
                SqlCommand cmd5 = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Status = 'Pending'", con);
                model.PendingApprovals = (int)cmd5.ExecuteScalar();

                // Overdue Borrowings
                SqlCommand cmd6 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE Status = 'Approved' AND DueDate < GETDATE()", con);
                model.OverdueBorrowings = (int)cmd6.ExecuteScalar();

                // Get Active Borrowings List
                SqlCommand cmd7 = new SqlCommand("SELECT RequestId, StudentName, BookName, RequestDate, ApprovedDate, DueDate, Status FROM BorrowRequests WHERE Status = 'Approved' ORDER BY DueDate", con);
                using (var reader = cmd7.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var req = new BorrowRequestModel();
                        req.RequestId = (int)reader["RequestId"];
                        req.StudentName = reader["StudentName"].ToString();
                        req.BookName = reader["BookName"].ToString();
                        req.RequestDate = (DateTime)reader["RequestDate"];
                        req.Status = reader["Status"].ToString();

                        if (reader["ApprovedDate"] != DBNull.Value)
                        {
                            req.ApprovedDate = (DateTime)reader["ApprovedDate"];
                        }

                        if (reader["DueDate"] != DBNull.Value)
                        {
                            req.DueDate = (DateTime)reader["DueDate"];
                        }

                        model.ActiveBorrowings.Add(req);
                    }
                }
            }

            return View(model);
        }
    }
}