using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Data;
using LibraryManagement.Models;
using System.Linq;

namespace LibraryManagement.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Book
        public IActionResult Index()
        {
            // This displays the list of books to everyone
            var books = _context.Books.ToList();
            return View(books);
        }

        // GET: Book/Details/5
        public IActionResult Details(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }
    }
}