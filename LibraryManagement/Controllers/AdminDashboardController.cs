using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LibraryManagement.Data;
using LibraryManagement.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace LibraryManagement.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // 1. DASHBOARD OVERVIEW
        // ==========================

        // Inside AdminDashboardController.cs -> Index()
        // Inside AdminDashboardController.cs

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");

            // 1. Total Books
            ViewBag.TotalBooks = _context.Books.Sum(b => (int?)b.Quantity) ?? 0;

            // 2. Total Users
            ViewBag.TotalUsers = _context.Users.Count();

            // 3. Pending Requests
            ViewBag.PendingRequests = _context.Users.Count(u => u.Status == "Pending");

            // 4. Total Fines Logic
            var activeOverdue = _context.Borrowing.Where(b => b.ReturnedDate == null && b.DueDate < DateTime.Now).ToList();
            int accumulating = activeOverdue.Sum(b => (DateTime.Now - b.DueDate.Value).Days * 10);
            int fixedFines = _context.Borrowing.Where(b => b.FineAmount > 0 && b.Status != "Paid").Sum(b => b.FineAmount ?? 0);
            ViewBag.TotalFines = accumulating + fixedFines;

            // 5. Issues Today
            ViewBag.IssuesToday = _context.Borrowing.Count(b => b.RequestDate.Date == DateTime.Today);

            // 6. User Chart Data
            int students = _context.Users.Count(u => u.Role == "Student");
            int admins = _context.Users.Count(u => u.Role == "Admin");
            int librarians = _context.Users.Count(u => u.Role == "Librarian");
            ViewBag.UserChartData = $"{students},{admins},{librarians}";

            // 7. Book Chart Data (Added this so your bar chart works)
            int totalStock = ViewBag.TotalBooks;
            int issuedBooks = _context.Borrowing.Count(b => b.ReturnedDate == null);
            ViewBag.BookChartData = $"{totalStock},{issuedBooks}";

            return View();
        }
        // ==========================
        // 2. OPERATIONS
        // ==========================
        public IActionResult Operations()
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public IActionResult UpdatePolicies(int maxBooks, int duration, int fineAmount)
        {
            TempData["Message"] = $"Policies Updated: Max Books {maxBooks}, Duration {duration}, Fine ${fineAmount}";
            return RedirectToAction("Operations");
        }

        [HttpPost]
        public IActionResult SendAlert(string type, string message)
        {
            TempData["Message"] = $"{type} Sent: {message}";
            return RedirectToAction("Operations");
        }

        [HttpPost]
        public IActionResult PostAnnouncement(string title, string content, string type)
        {
            _context.Announcements.Add(new Announcement { Title = title, Content = content, Type = type, PostedDate = DateTime.Now });
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ==========================
        // 3. PROFILE
        // ==========================
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");
            var email = HttpContext.Session.GetString("Email");
            var admin = _context.Users.FirstOrDefault(u => u.Email == email);
            return View(admin);
        }

        // ==========================
        // 4. USER MANAGEMENT
        // ==========================
        public IActionResult ManageUsers() => View(_context.Users.ToList());

        [HttpGet] public IActionResult EditUser(int id) => View(_context.Users.Find(id));

        [HttpPost]
        public IActionResult EditUser(User u)
        {
            var user = _context.Users.Find(u.UserId);
            if (user != null)
            {
                user.Name = u.Name; user.Email = u.Email; user.Phone = u.Phone;
                user.Role = u.Role; user.Status = u.Status;
                _context.SaveChanges();
                return RedirectToAction("ManageUsers");
            }
            return View(u);
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null) { _context.Users.Remove(user); _context.SaveChanges(); }
            return RedirectToAction("ManageUsers");
        }

        public IActionResult ResetPassword(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null) { user.Password = "123456"; _context.SaveChanges(); }
            return RedirectToAction("ManageUsers");
        }

        public IActionResult ToggleStatus(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null) { user.Status = user.Status == "Active" ? "Inactive" : "Active"; _context.SaveChanges(); }
            return RedirectToAction("ManageUsers");
        }

        // ==========================
        // 5. BOOK MANAGEMENT
        // ==========================
        public IActionResult ManageBooks() => View(_context.Books.ToList());
        [HttpGet] public IActionResult AddBook() => View();
        [HttpPost] public IActionResult AddBook(Book b) { _context.Books.Add(b); _context.SaveChanges(); return RedirectToAction("ManageBooks"); }

        // ==========================
        // 6. ISSUE DESK
        // ==========================
        public IActionResult IssueDesk() => View(_context.Borrowing.Where(b => b.ReturnedDate == null).OrderBy(b => b.DueDate).ToList());

        [HttpPost]
        public IActionResult QuickIssue(string username, string bookTitle)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var book = _context.Books.FirstOrDefault(b => b.Title == bookTitle);
            if (user != null && book != null)
            {
                _context.Borrowing.Add(new Borrowing { StudentName = user.Name, BookName = book.Title, RequestDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14), Status = "Approved", ApprovedDate = DateTime.Now });
                book.Quantity--;
                _context.SaveChanges();
            }
            return RedirectToAction("IssueDesk");
        }

        public IActionResult MarkReturned(int id)
        {
            var loan = _context.Borrowing.Find(id);
            if (loan != null) { loan.ReturnedDate = DateTime.Now; loan.Status = "Returned"; _context.SaveChanges(); }
            return RedirectToAction("IssueDesk");
        }

        // ==========================
        // 7. FINES MANAGEMENT (FIXED)
        // ==========================
        public IActionResult Fines()
        {
            // Logic:
            // 1. Get books that are currently overdue (ReturnedDate is null)
            // 2. Get books that HAVE fines (FineAmount > 0) AND are not yet 'Paid' (Status != 'Paid')
            // This covers "Lost" books because they have FineAmount=500 and Status="Lost"

            var finesList = _context.Borrowing
                .Where(b =>
                    (b.ReturnedDate == null && b.DueDate < DateTime.Now) ||
                    (b.FineAmount > 0 && b.Status != "Paid")
                )
                .OrderByDescending(b => b.FineAmount)
                .ToList();

            return View(finesList);
        }

        // NEW: ACTION TO PAY FINE
        public IActionResult CollectFine(int id)
        {
            var record = _context.Borrowing.Find(id);
            if (record != null)
            {
                // We keep the Amount for history, but mark status as Paid
                record.Status = "Paid";
                _context.SaveChanges();
            }
            return RedirectToAction("Fines");
        }
    }
}