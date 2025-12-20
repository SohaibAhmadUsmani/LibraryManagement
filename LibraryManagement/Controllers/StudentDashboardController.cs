using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class StudentDashboardController : Controller
    {
        private readonly IConfiguration _config;

        public StudentDashboardController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            // Check if user is logged in and is a student
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (role != "Student" || userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new StudentDashboardModel
            {
                StudentName = HttpContext.Session.GetString("FullName") ?? "Student"
            };

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Get student's borrowed books count
            var borrowedCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Borrowings b INNER JOIN Students s ON s.StudentName = b.StudentName INNER JOIN Users u ON u.UserId = s.UserId WHERE u.UserId = @UserId",
                con);
            borrowedCmd.Parameters.AddWithValue("@UserId", userId.Value);
            model.MyBorrowedBooks = (int)borrowedCmd.ExecuteScalar();

            // Get total available books
            var availableCmd = new SqlCommand("SELECT COUNT(*) FROM Books", con);
            model.AvailableBooks = (int)availableCmd.ExecuteScalar();

            // Get recent books (top 5)
            var recentBooksCmd = new SqlCommand("SELECT TOP 5 * FROM Books ORDER BY BookId DESC", con);
            using var reader = recentBooksCmd.ExecuteReader();
            while (reader.Read())
            {
                model.RecentBooks.Add(new BookModel
                {
                    BookId = (int)reader["BookId"],
                    BookName = reader["BookName"].ToString() ?? "",
                    Author = reader["Author"].ToString() ?? "",
                    Publisher = reader["Publisher"].ToString() ?? "",
                    Quantity = (int)reader["Quantity"]
                });
            }

            return View(model);
        }
    }
}
