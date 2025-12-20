using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagement.Models;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Verify(LoginModel usr)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", usr);
            }

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand("SELECT UserId, Username, Role, FullName FROM Users WHERE Username = @Username AND Password = @Password AND IsActive = 1", con);
            cmd.Parameters.AddWithValue("@Username", usr.username);
            cmd.Parameters.AddWithValue("@Password", usr.password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // Store user info in session
                HttpContext.Session.SetInt32("UserId", (int)reader["UserId"]);
                HttpContext.Session.SetString("Username", reader["Username"].ToString() ?? "");
                HttpContext.Session.SetString("Role", reader["Role"].ToString() ?? "");
                HttpContext.Session.SetString("FullName", reader["FullName"].ToString() ?? "");

                string role = reader["Role"].ToString() ?? "";

                TempData["message"] = "Login Success";

                // Redirect based on role
                return role.ToLower() switch
                {
                    "admin" => RedirectToAction("Index", "Dashboard"),
                    "student" => RedirectToAction("Index", "StudentDashboard"),
                    "librarian" => RedirectToAction("Index", "LibrarianDashboard"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            else
            {
                ViewBag.message = "Invalid username or password!";
                return View("Index");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}