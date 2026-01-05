using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LibraryManagement.Data;
using LibraryManagement.Models;
using System.Linq;
using System;

namespace LibraryManagement.Controllers
{
    public class LibrarianDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibrarianDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DASHBOARD (Matches Views/LibrarianDashboard/Index.cshtml)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Librarian") return RedirectToAction("Login", "Account");

            ViewBag.IssuedBooks = _context.Borrowing.Count(b => b.ReturnedDate == null);
            ViewBag.ReturnsDueToday = _context.Borrowing.Count(b => b.ReturnedDate == null && b.DueDate.Value.Date == DateTime.Today);
            ViewBag.OverdueBooks = _context.Borrowing.Count(b => b.ReturnedDate == null && b.DueDate < DateTime.Now);
            ViewBag.TotalInventory = _context.Books.Sum(b => (int?)b.Quantity) ?? 0;

            return View();
        }

        // 2. INVENTORY (Matches Views/LibrarianDashboard/Inventory.cshtml)
        public IActionResult Inventory()
        {
            var books = _context.Books.ToList();
            return View(books);
        }

        // 3. BOOK REQUESTS (Matches Views/LibrarianDashboard/BookRequests.cshtml)
        public IActionResult BookRequests()
        {
            var requests = _context.Borrowing.Where(b => b.Status == "Pending").ToList();
            return View(requests);
        }

        // 4. RETURN BOOK (Matches Views/LibrarianDashboard/ReturnBook.cshtml)
        public IActionResult ReturnBook()
        {
            var activeLoans = _context.Borrowing.Where(b => b.ReturnedDate == null && b.Status == "Approved").ToList();
            return View(activeLoans);
        }

        // 5. ACTIVE BORROWINGS (Matches Views/LibrarianDashboard/ActiveBorrowings.cshtml)
        public IActionResult ActiveBorrowings()
        {
            var borrowed = _context.Borrowing.Where(b => b.ReturnedDate == null).ToList();
            return View(borrowed);
        }

        // 6. STUDENT LOOKUP (Matches Views/LibrarianDashboard/StudentLookup.cshtml)
        public IActionResult StudentLookup()
        {
            var students = _context.Users.Where(u => u.Role == "Student").ToList();
            return View(students);
        }

        // --- ACTIONS (Logic for Approve/Return buttons) ---
        public IActionResult ApproveRequest(int id)
        {
            var req = _context.Borrowing.Find(id);
            if (req != null)
            {
                req.Status = "Approved";
                req.ApprovedDate = DateTime.Now;
                req.DueDate = DateTime.Now.AddDays(14);

                var book = _context.Books.FirstOrDefault(b => b.Title == req.BookName);
                if (book != null && book.Quantity > 0) book.Quantity--;

                _context.SaveChanges();
            }
            return RedirectToAction("BookRequests");
        }

        public IActionResult ConfirmReturn(int id)
        {
            var req = _context.Borrowing.Find(id);
            if (req != null)
            {
                req.Status = "Returned";
                req.ReturnedDate = DateTime.Now;

                var book = _context.Books.FirstOrDefault(b => b.Title == req.BookName);
                if (book != null) book.Quantity++;

                _context.SaveChanges();
            }
            return RedirectToAction("ReturnBook");
        }
    }
}