using LibraryManagement.Data;
using LibraryManagement.Models; // Pointing to the correct namespace
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _context;

        public AccountController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                if (!user.IsActive || user.Status == "Rejected")
                {
                    ViewBag.Error = "Your account is not active.";
                    return View();
                }

                // Store Session Data
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("AccessLevel", user.AccessLevel);

                // Redirect based on AccessLevel
                if (user.AccessLevel == 3) return RedirectToAction("Index", "Dashboard"); // Admin
                if (user.AccessLevel == 2) return RedirectToAction("Index", "LibrarianDashboard"); // Librarian

                return RedirectToAction("Index", "StudentDashboard"); // Student
            }

            ViewBag.Error = "Invalid Credentials";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserModel model, string userType)
        {
            // Set Access Level based on radio button or selection
            model.AccessLevel = 1; // Default Student
            model.Status = "Active";

            if (userType == "Librarian")
            {
                model.AccessLevel = 2;
                model.Status = "Pending";
            }
            // Admin (Level 3) is usually created manually in DB or via a secret code

            // Note: We don't set "Role" string anymore, we set AccessLevel

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}