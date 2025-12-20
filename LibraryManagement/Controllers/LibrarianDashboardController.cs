using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class LibrarianDashboardController : Controller
    {
        private readonly IConfiguration _config;

        public LibrarianDashboardController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            // Check if user is logged in and is a librarian
            var role = HttpContext.Session.GetString("Role");
            if (role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new LibrarianDashboardModel
            {
                LibrarianName = HttpContext.Session.GetString("FullName") ?? "Librarian"
            };

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Get counts
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Students", con))
                model.TotalStudents = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Books", con))
                model.TotalBooks = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Borrowings", con))
                model.TotalBorrowings = (int)cmd.ExecuteScalar();

            // Get recent borrowings
            var recentCmd = new SqlCommand("SELECT TOP 5 * FROM Borrowings ORDER BY BorrowingId DESC", con);
            using var reader = recentCmd.ExecuteReader();
            while (reader.Read())
            {
                model.RecentBorrowings.Add(new BorrowingModel
                {
                    BorrowingId = (int)reader["BorrowingId"],
                    StudentName = reader["StudentName"].ToString() ?? "",
                    BookName = reader["BookName"].ToString() ?? "",
                    BorrowDate = (DateTime)reader["BorrowDate"],
                    ReturnedDate = (DateTime)reader["ReturnedDate"]
                });
            }

            return View(model);
        }
    }
}
