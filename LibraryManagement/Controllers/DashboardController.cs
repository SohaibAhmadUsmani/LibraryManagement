using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;
using System.Linq;

namespace LibraryManagement.Controllers
{
    
    public class DashboardController : Controller
    {
        private readonly string _connectionString = "Server=DESKTOP-DASLLN7;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public IActionResult Index()
        {

            var model = new DashboardModel();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Count Students
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Students", connection))
                {
                    model.TotalStudents = (int)cmd.ExecuteScalar();
                }



                // Count Books
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Books", connection))
                {
                    model.TotalBooks = (int)cmd.ExecuteScalar();
                }

                // Count Librarians
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Librarians", connection))
                {
                    model.TotalLibrarians = (int)cmd.ExecuteScalar();
                }

                // Count Borrowings
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Borrowings", connection))
                {
                    model.TotalBorrowings = (int)cmd.ExecuteScalar();
                }
            }

            return View(model);
        }
    }
}
