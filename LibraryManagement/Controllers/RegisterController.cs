using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IConfiguration _config;

        public RegisterController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username or email already exists
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email", con);
            checkCmd.Parameters.AddWithValue("@Username", model.Username);
            checkCmd.Parameters.AddWithValue("@Email", model.Email);

            int existingUsers = (int)checkCmd.ExecuteScalar();
            if (existingUsers > 0)
            {
                ViewBag.Error = "Username or Email already exists!";
                return View(model);
            }

            // Insert new user
            var insertCmd = new SqlCommand(
                "INSERT INTO Users (Username, Email, Password, Role, FullName, Phone) VALUES (@Username, @Email, @Password, @Role, @FullName, @Phone)",
                con);

            insertCmd.Parameters.AddWithValue("@Username", model.Username);
            insertCmd.Parameters.AddWithValue("@Email", model.Email);
            insertCmd.Parameters.AddWithValue("@Password", model.Password); // In real app, hash this!
            insertCmd.Parameters.AddWithValue("@Role", model.Role);
            insertCmd.Parameters.AddWithValue("@FullName", model.FullName);
            insertCmd.Parameters.AddWithValue("@Phone", model.Phone);

            insertCmd.ExecuteNonQuery();

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Index", "Login");
        }
    }
}
