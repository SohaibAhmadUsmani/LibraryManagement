using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Data;
using LibraryManagement.Models;
using System.Linq;

namespace LibraryManagement.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(User user)
        {
            // 1. Check if username or email already exists
            if (_context.Users.Any(u => u.Username == user.Username || u.Email == user.Email))
            {
                ViewBag.Error = "Username or Email already exists.";
                return View("Index", user);
            }

            // 2. Set Default Values
            user.Role = "Student";       // Default role
            user.Status = "Pending";     // Default status (Admin must approve)
            user.CreatedAt = System.DateTime.Now;

            // 3. Save to Database
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login", "Account");
            }

            return View("Index", user);
        }
    }
}