using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using LibraryManagement.Data;

namespace LibraryManagement.Controllers
{
    // FIX: Using Primary Constructor syntax (fixes "Use primary constructor" suggestions)
    public class StudentDashboardController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        // 1. UPDATE INDEX METHOD
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Student") return RedirectToAction("Login", "Account");
            string studentName = HttpContext.Session.GetString("Name");

            // 1. Borrowed
            ViewBag.BorrowedCount = _context.Borrowing.Count(b => b.StudentName == studentName && b.ReturnedDate == null);
            // 2. History
            ViewBag.HistoryCount = _context.Borrowing.Count(b => b.StudentName == studentName && b.ReturnedDate != null);
            // 3. Due Soon (3 days)
            DateTime limit = DateTime.Now.AddDays(3);
            ViewBag.DueSoon = _context.Borrowing.Count(b => b.StudentName == studentName && b.ReturnedDate == null && b.DueDate <= limit);
            // 4. Fines
            var myOverdue = _context.Borrowing.Where(b => b.StudentName == studentName && b.ReturnedDate == null && b.DueDate < DateTime.Now).ToList();
            int fines = myOverdue.Sum(b => (DateTime.Now - b.DueDate.Value).Days * 10);
            ViewBag.MyFines = fines + (_context.Borrowing.Where(b => b.StudentName == studentName).Sum(b => (int?)b.FineAmount) ?? 0);
            // 5. Lost Books (Placeholder stat)
            ViewBag.LostBooks = _context.Borrowing.Count(b => b.StudentName == studentName && b.Status == "Lost");

            // Announcements
            ViewBag.Announcements = _context.Announcements.OrderByDescending(a => a.PostedDate).Take(3).ToList();

            return View();
        }

        // 2. ADD NEW ACTION: Report Lost
        [HttpPost]
        public IActionResult ReportLost(int id)
        {
            var loan = _context.Borrowing.FirstOrDefault(b => b.RequestId == id);
            if (loan != null)
            {
                loan.Status = "Lost";
                loan.ReturnedDate = DateTime.Now; // Loop closed
                loan.FineAmount = 500; // Fixed Penalty for Lost Book

                _context.SaveChanges();
                TempData["Error"] = "Book reported LOST. Fine of $500 applied.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult BrowseBooks()
        {
            // Sending the list of Books to the view
            var books = _context.Books.Where(b => b.Quantity > 0).ToList();
            return View(books);
        }

        public IActionResult RequestBook(int id)
        {
            var username = HttpContext.Session.GetString("Name");
            if (username == null) return RedirectToAction("Login", "Account");

            var book = _context.Books.Find(id);
            if (book != null && book.Quantity > 0)
            {
                bool alreadyHas = _context.Borrowing.Any(b => b.StudentName == username && b.BookName == book.Title && b.ReturnedDate == null);

                if (alreadyHas)
                {
                    TempData["Error"] = "You already have a copy of this book!";
                    return RedirectToAction("BrowseBooks");
                }

                var req = new Borrowing
                {
                    StudentName = username,
                    BookName = book.Title,
                    RequestDate = DateTime.Now,
                    Status = "Pending",
                    DueDate = DateTime.Now.AddDays(14)
                };

                _context.Borrowing.Add(req);
                _context.SaveChanges();
                TempData["Success"] = "Book Requested Successfully!";
            }
            return RedirectToAction("BrowseBooks");
        }

        public IActionResult MyHistory()
        {
            var username = HttpContext.Session.GetString("Name");
            var history = _context.Borrowing
                .Where(b => b.StudentName == username)
                .OrderByDescending(b => b.RequestDate)
                .ToList();
            return View(history);
        }

        // 5. MY PROFILE ACTION
        // 5. MY PROFILE (FIXED)
        public IActionResult Profile()
        {
            // 1. Get the username from the session
            var activeUser = HttpContext.Session.GetString("Name");

            // Safety Check: If session is empty, go to login
            if (string.IsNullOrEmpty(activeUser))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. SMARTER LOOKUP: Check if the session stored the 'Username' OR the 'Name'
            // This fixes the issue where it couldn't find the user.
            var user = _context.Users.FirstOrDefault(u => u.Username == activeUser || u.Name == activeUser);

            // 3. If we still can't find the user in the DB, show an error instead of a login page
            if (user == null)
            {
                ViewBag.ErrorMessage = "User details not found in database.";
                return View("Error"); // Optional: You can route to a generic error page
            }

            // 4. Send the user data to the View
            return View(user);
        }
    }
}