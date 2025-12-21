using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var students = new List<StudentModel>();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Get students from Users table
            using var cmd = new SqlCommand(@"
                SELECT u.UserId, u. FullName, u.Email, u.Phone, u.Username, u.Status, u.CreatedDate,
                       s.StudentId
                FROM Users u
                LEFT JOIN Students s ON u.UserId = s.UserId
                WHERE u.Role = 'Student' AND u.IsActive = 1
                ORDER BY u. FullName", con);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new StudentModel
                {
                    StudentId = reader["StudentId"] != DBNull.Value ? (int)reader["StudentId"] : 0,
                    StudentName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? ""
                });
            }

            return View(students);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(StudentModel model, string Username, string Password)
        {
            if (string.IsNullOrEmpty(model.StudentName) || string.IsNullOrEmpty(model.Email))
            {
                ViewBag.Error = "Name and Email are required. ";
                return View(model);
            }

            // Set default password if not provided
            if (string.IsNullOrEmpty(Password))
            {
                Password = "123456";
            }

            // Set default username if not provided
            if (string.IsNullOrEmpty(Username))
            {
                Username = model.Email.Split('@')[0];
            }

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Check if username or email already exists
            using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email", con))
            {
                checkCmd.Parameters.AddWithValue("@Username", Username);
                checkCmd.Parameters.AddWithValue("@Email", model.Email);
                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    ViewBag.Error = "Username or Email already exists.";
                    return View(model);
                }
            }

            // Insert into Users table first
            int userId = 0;
            using (var userCmd = new SqlCommand(@"
                INSERT INTO Users (Username, Email, Password, Role, Status, FullName, Phone, IsActive, CreatedDate) 
                OUTPUT INSERTED.UserId
                VALUES (@Username, @Email, @Password, 'Student', 'Approved', @FullName, @Phone, 1, GETDATE())", con))
            {
                userCmd.Parameters.AddWithValue("@Username", Username);
                userCmd.Parameters.AddWithValue("@Email", model.Email);
                userCmd.Parameters.AddWithValue("@Password", Password);
                userCmd.Parameters.AddWithValue("@FullName", model.StudentName);
                userCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                userId = (int)userCmd.ExecuteScalar();
            }

            // Insert into Students table
            using (var studentCmd = new SqlCommand(@"
                INSERT INTO Students (StudentName, Email, Phone, UserId) 
                VALUES (@Name, @Email, @Phone, @UserId)", con))
            {
                studentCmd.Parameters.AddWithValue("@Name", model.StudentName);
                studentCmd.Parameters.AddWithValue("@Email", model.Email);
                studentCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                studentCmd.Parameters.AddWithValue("@UserId", userId);
                studentCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Student created successfully!  Username: " + Username + ", Password: " + Password;
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var model = new StudentModel();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new SqlCommand("SELECT * FROM Students WHERE StudentId = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.StudentId = (int)reader["StudentId"];
                model.StudentName = reader["StudentName"]?.ToString() ?? "";
                model.Email = reader["Email"]?.ToString() ?? "";
                model.Phone = reader["Phone"]?.ToString() ?? "";
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(StudentModel model)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Update Students table
            var cmd = new SqlCommand("UPDATE Students SET StudentName=@Name, Email=@Email, Phone=@Phone WHERE StudentId=@Id", con);
            cmd.Parameters.AddWithValue("@Id", model.StudentId);
            cmd.Parameters.AddWithValue("@Name", model.StudentName ?? "");
            cmd.Parameters.AddWithValue("@Email", model.Email ?? "");
            cmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            cmd.ExecuteNonQuery();

            // Also update Users table if linked
            var updateUserCmd = new SqlCommand(@"
                UPDATE Users SET FullName = @Name, Email = @Email, Phone = @Phone 
                WHERE UserId = (SELECT UserId FROM Students WHERE StudentId = @Id)", con);
            updateUserCmd.Parameters.AddWithValue("@Id", model.StudentId);
            updateUserCmd.Parameters.AddWithValue("@Name", model.StudentName ?? "");
            updateUserCmd.Parameters.AddWithValue("@Email", model.Email ?? "");
            updateUserCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            updateUserCmd.ExecuteNonQuery();

            TempData["Success"] = "Student updated successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Get UserId first
            int userId = 0;
            using (var getCmd = new SqlCommand("SELECT UserId FROM Students WHERE StudentId = @Id", con))
            {
                getCmd.Parameters.AddWithValue("@Id", id);
                var result = getCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    userId = (int)result;
            }

            // Delete from Students
            var cmd = new SqlCommand("DELETE FROM Students WHERE StudentId = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            // Deactivate user instead of deleting
            if (userId > 0)
            {
                var deactivateCmd = new SqlCommand("UPDATE Users SET IsActive = 0 WHERE UserId = @UserId", con);
                deactivateCmd.Parameters.AddWithValue("@UserId", userId);
                deactivateCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Student deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}