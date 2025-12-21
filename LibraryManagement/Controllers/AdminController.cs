using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _config;

        public AdminController(IConfiguration config)
        {
            _config = config;
        }

        // Helper:  Check if current user is Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role?.ToLower() == "admin";
        }

        // View pending librarian approval requests
        public IActionResult PendingApprovals()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Index", "Login");
            }

            var pendingUsers = new List<UserModel>();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT UserId, Username, Email, FullName, Phone, Role, CreatedDate 
                FROM Users 
                WHERE Status = 'Pending' AND Role = 'Librarian'
                ORDER BY CreatedDate DESC", con);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pendingUsers.Add(new UserModel
                {
                    UserId = (int)reader["UserId"],
                    Username = reader["Username"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    FullName = reader["FullName"].ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    Role = reader["Role"].ToString() ?? "",
                    CreatedDate = (DateTime)reader["CreatedDate"]
                });
            }

            ViewBag.PendingCount = pendingUsers.Count;
            return View(pendingUsers);
        }

        // Approve a librarian
        [HttpPost]
        public IActionResult ApproveUser(int userId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Login");
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                UPDATE Users 
                SET Status = 'Approved', ApprovedDate = GETDATE(), ApprovedBy = @AdminId 
                WHERE UserId = @UserId AND Status = 'Pending'", con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@AdminId", adminId);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                TempData["Success"] = "✅ Librarian approved successfully!  They can now login.";
            }
            else
            {
                TempData["Error"] = "User not found or already processed.";
            }

            return RedirectToAction("PendingApprovals");
        }

        // Reject a librarian
        [HttpPost]
        public IActionResult RejectUser(int userId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Login");
            }

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                UPDATE Users 
                SET Status = 'Rejected', IsActive = 0 
                WHERE UserId = @UserId AND Status = 'Pending'", con);
            cmd.Parameters.AddWithValue("@UserId", userId);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                TempData["Success"] = "❌ Librarian request rejected. ";
            }

            return RedirectToAction("PendingApprovals");
        }

        // View all users in system
        public IActionResult ManageUsers()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Login");
            }

            var users = new List<UserModel>();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT UserId, Username, Email, FullName, Phone, Role, Status, CreatedDate, IsActive 
                FROM Users 
                WHERE Role != 'Admin'
                ORDER BY CreatedDate DESC", con);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new UserModel
                {
                    UserId = (int)reader["UserId"],
                    Username = reader["Username"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    FullName = reader["FullName"].ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    Role = reader["Role"].ToString() ?? "",
                    Status = reader["Status"]?.ToString() ?? "",
                    CreatedDate = (DateTime)reader["CreatedDate"],
                    IsActive = (bool)reader["IsActive"]
                });
            }

            return View(users);
        }

        // Toggle user active status
        [HttpPost]
        public IActionResult ToggleUserStatus(int userId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Login");
            }

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand(@"
                UPDATE Users 
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END 
                WHERE UserId = @UserId", con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();

            TempData["Success"] = "User status updated. ";
            return RedirectToAction("ManageUsers");
        }
    }
}