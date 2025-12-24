using LibraryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class BookController : Controller
    {
        private readonly LibraryContext _context;

        public BookController(LibraryContext context)
        {
            _context = context;
        }

        // Helper to check access (Level 2+ required for managing books)
        private bool HasAccess()
        {
            int? level = HttpContext.Session.GetInt32("AccessLevel");
            return level != null && level >= 2;
        }

        public IActionResult Index()
        {
            if (!HasAccess()) return RedirectToAction("Login", "Account");

            // EF Core: Simple .ToList()
            var books = _context.Books.ToList();
            return View(books);
        }

        public IActionResult Create()
        {
            if (!HasAccess()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public IActionResult Create(BookModel book)
        {
            if (!HasAccess()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                _context.SaveChanges(); // Commits to DB
                return RedirectToAction("Index");
            }
            return View(book);
        }
    }
}