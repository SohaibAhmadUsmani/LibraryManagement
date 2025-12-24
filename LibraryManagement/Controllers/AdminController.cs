using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Data;
using Microsoft.EntityFrameworkCore; // Required for EF Core features

namespace LibraryManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;

        public AdminController(LibraryContext context)
        {
            _context = context;
        }

        // Helper: Check if current user is Admin (AccessLevel 3)
        private bool IsAdmin()
        {
            int? level = HttpContext.Session.GetInt32("AccessLevel");
            return level != null && level == 3;
        }

        // View pending librarian approval requests
        public IActionResult PendingApprovals()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Login", "Account");
            }

            // EF Core Logic: Find Librarians (Level 2) who are "Pending"
            var pendingUsers = _context.Users
                .Where(u => u.Status == "Pending" && u.AccessLevel == 2)
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            ViewBag.PendingCount = pendingUsers.Count;
            return View(pendingUsers);
        }

        // Approve a librarian
        [HttpPost]
        public IActionResult ApproveUser(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);
            if (user != null && user.Status == "Pending")
            {
                user.Status = "Approved";
                // We don't need ApprovedDate/ApprovedBy unless you add those fields to UserModel.
                // If you really need them, let me know, otherwise this is fine.

                _context.SaveChanges(); // Updates database
                TempData["Success"] = "✅ Librarian approved successfully!";
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
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.Status = "Rejected";
                user.IsActive = false;

                _context.SaveChanges();
                TempData["Success"] = "❌ Librarian request rejected.";
            }

            return RedirectToAction("PendingApprovals");
        }

        // View all users in system
        public IActionResult ManageUsers()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Get all users who are NOT Admins (AccessLevel != 3)
            var users = _context.Users
                .Where(u => u.AccessLevel != 3)
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            return View(users);
        }

        // Toggle user active status
        [HttpPost]
        public IActionResult ToggleUserStatus(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive; // Flip true/false
                _context.SaveChanges();
                TempData["Success"] = "User status updated.";
            }

            return RedirectToAction("ManageUsers");

        }
    }
}