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

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Check if username or email already exists
            var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email", con);
            checkCmd.Parameters.AddWithValue("@Username", model.Username);
            checkCmd.Parameters.AddWithValue("@Email", model.Email);

            int existingUsers = (int)checkCmd.ExecuteScalar();
            if (existingUsers > 0)
            {
                ViewBag.Error = "Username or Email already exists!";
                return View(model);
            }

            
            string status = model.Role.ToLower() == "student" ? "Approved" : "Pending";

            // Insert new user 
            var insertCmd = new SqlCommand(@"
                INSERT INTO Users (Username, Email, Password, Role, Status, FullName, Phone, CreatedDate, IsActive) 
                OUTPUT INSERTED.UserId
                VALUES (@Username, @Email, @Password, @Role, @Status, @FullName, @Phone, GETDATE(), 1)",
                con);

            insertCmd.Parameters.AddWithValue("@Username", model.Username);
            insertCmd.Parameters.AddWithValue("@Email", model.Email);
            insertCmd.Parameters.AddWithValue("@Password", model.Password); 
            insertCmd.Parameters.AddWithValue("@Role", model.Role);
            insertCmd.Parameters.AddWithValue("@Status", status);
            insertCmd.Parameters.AddWithValue("@FullName", model.FullName);
            insertCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");

            // Get the newly created UserId
            int newUserId = (int)insertCmd.ExecuteScalar();

   
            if (model.Role.ToLower() == "student")
            {
                var studentCmd = new SqlCommand(@"
                INSERT INTO Students (StudentName, Email, Phone, UserId) 
                lVALUES (@Name, @Email, @Phone, @UserId)", con);
                studentCmd.Parameters.AddWithValue("@Name", model.FullName);
                studentCmd.Parameters.AddWithValue("@Email", model.Email);
                studentCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                studentCmd.Parameters.AddWithValue("@UserId", newUserId);
                studentCmd.ExecuteNonQuery();
            }
            else if (model.Role.ToLower() == "librarian")
            {
                // Insert into Librarians table 
                var librarianCmd = new SqlCommand(@"
                    INSERT INTO Librarians (Name, Phone, UserId) 
                    VALUES (@Name, @Phone, @UserId)", con);
                    librarianCmd.Parameters.AddWithValue("@Name", model.FullName);
                     librarianCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                    librarianCmd.Parameters.AddWithValue("@UserId", newUserId);
                    librarianCmd.ExecuteNonQuery();
            }

        
            if (model.Role.ToLower() == "librarian")
            {
                TempData["Message"] = "Your librarian account is pending admin approval. You will be notified once approved.";
                return RedirectToAction("PendingApproval");
            }

            TempData["Success"] = "Registration successful!  Please login.";
            return RedirectToAction("Index", "Login");
        }
            public IActionResult PendingApproval()
        {
            return View();
        }
    }
}