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

            // ✅ Fetch Status along with other fields
            var cmd = new SqlCommand(@"
                SELECT UserId, Username, Role, FullName, Status, IsActive 
                FROM Users 
                WHERE Username = @Username AND Password = @Password", con);
            cmd.Parameters.AddWithValue("@Username", usr.username);
            cmd.Parameters.AddWithValue("@Password", usr.password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                bool isActive = (bool)reader["IsActive"];
                string status = reader["Status"]?.ToString() ?? "Pending";
                string role = reader["Role"].ToString() ?? "";

                // ✅ Check if account is active
                if (!isActive)
                {
                    ViewBag.message = "Your account has been deactivated. Please contact administrator.";
                    return View("Index");
                }

                // ✅ CHECK STATUS - This prevents unapproved librarians from logging in! 
                if (status == "Pending")
                {
                    ViewBag.message = "⏳ Your account is pending admin approval. Please wait for approval.";
                    return View("Index");
                }

                if (status == "Rejected")
                {
                    ViewBag.message = "❌ Your account request was rejected. Please contact administrator.";
                    return View("Index");
                }

                // ✅ Status is "Approved" - Allow login
                HttpContext.Session.SetInt32("UserId", (int)reader["UserId"]);
                HttpContext.Session.SetString("Username", reader["Username"].ToString() ?? "");
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("FullName", reader["FullName"].ToString() ?? "");

                TempData["message"] = "Login Success!  Welcome " + reader["FullName"].ToString();

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
                ViewBag.message = "Invalid username or password! ";
                return View("Index");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["message"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Login");
        }
    }
}