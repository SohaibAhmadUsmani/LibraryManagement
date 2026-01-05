using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LibraryManagement.Data;
using LibraryManagement.Models;
using System.Linq;

namespace LibraryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Process Login (Using USERNAME and PASSWORD)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
           
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // 2. Set Session
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // 3. Redirect to the correct Dashboard
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else if (user.Role == "Student")
                {
                    return RedirectToAction("Index", "StudentDashboard");
                }
                else if (user.Role == "Librarian")
                {
      
                    return RedirectToAction("Index", "LibrarianDashboard");
                }
            }

            // 4. If login fails
            ViewBag.Error = "Invalid Username or Password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}