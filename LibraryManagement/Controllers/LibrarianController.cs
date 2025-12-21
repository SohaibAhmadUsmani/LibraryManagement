using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class LibrarianController : Controller
    {
        private readonly IConfiguration _config;

        public LibrarianController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var librarians = new List<LibrarianModel>();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Get librarians from Users table - use UserId as LibrarianId if LibrarianId is 0
            using var cmd = new SqlCommand(@"
                SELECT u.UserId, u.FullName, u.Email, u.Phone, u. Status, u.CreatedDate,
                       ISNULL(l.LibrarianId, u.UserId) as LibrarianId, ISNULL(l.Age, 0) as Age
                FROM Users u
                LEFT JOIN Librarians l ON u.UserId = l.UserId
                WHERE u.Role = 'Librarian' AND u.Status = 'Approved' AND u.IsActive = 1
                ORDER BY u.FullName", con);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                librarians.Add(new LibrarianModel
                {
                    LibrarianId = (int)reader["LibrarianId"],
                    Name = reader["FullName"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    Age = (int)reader["Age"]
                });
            }

            return View(librarians);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(LibrarianModel model, string Username, string Password)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                ViewBag.Error = "Name is required.";
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
                Username = model.Name.ToLower().Replace(" ", "");
            }

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Check if username already exists
            using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", con))
            {
                checkCmd.Parameters.AddWithValue("@Username", Username);
                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    ViewBag.Error = "Username already exists.";
                    return View(model);
                }
            }

            // Insert into Users table first
            int userId = 0;
            using (var userCmd = new SqlCommand(@"
                INSERT INTO Users (Username, Email, Password, Role, Status, FullName, Phone, IsActive, CreatedDate) 
                OUTPUT INSERTED.UserId
                VALUES (@Username, @Email, @Password, 'Librarian', 'Approved', @FullName, @Phone, 1, GETDATE())", con))
            {
                userCmd.Parameters.AddWithValue("@Username", Username);
                userCmd.Parameters.AddWithValue("@Email", Username + "@library.com");
                userCmd.Parameters.AddWithValue("@Password", Password);
                userCmd.Parameters.AddWithValue("@FullName", model.Name);
                userCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                userId = (int)userCmd.ExecuteScalar();
            }

            // Insert into Librarians table
            using (var libCmd = new SqlCommand(@"
                INSERT INTO Librarians (Name, Age, Phone, UserId) 
                VALUES (@Name, @Age, @Phone, @UserId)", con))
            {
                libCmd.Parameters.AddWithValue("@Name", model.Name);
                libCmd.Parameters.AddWithValue("@Age", model.Age);
                libCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                libCmd.Parameters.AddWithValue("@UserId", userId);
                libCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Librarian created successfully! Username: " + Username + ", Password: " + Password;
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var model = new LibrarianModel();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Try to get from Librarians table first
            var cmd = new SqlCommand(@"
                SELECT l.LibrarianId, l.Name, l.Age, l.Phone 
                FROM Librarians l 
                WHERE l. LibrarianId = @Id
                UNION
                SELECT u.UserId as LibrarianId, u.FullName as Name, 0 as Age, u.Phone
                FROM Users u
                WHERE u.UserId = @Id AND u.Role = 'Librarian'", con);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.LibrarianId = (int)reader["LibrarianId"];
                model.Name = reader["Name"]?.ToString() ?? "";
                model.Age = reader["Age"] != DBNull.Value ? (int)reader["Age"] : 0;
                model.Phone = reader["Phone"]?.ToString() ?? "";
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(LibrarianModel model)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Update Librarians table
            var cmd = new SqlCommand("UPDATE Librarians SET Name=@Name, Age=@Age, Phone=@Phone WHERE LibrarianId=@Id", con);
            cmd.Parameters.AddWithValue("@Id", model.LibrarianId);
            cmd.Parameters.AddWithValue("@Name", model.Name ?? "");
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            cmd.ExecuteNonQuery();

            // Also update Users table
            var updateUserCmd = new SqlCommand(@"
                UPDATE Users SET FullName = @Name, Phone = @Phone 
                WHERE UserId = (SELECT UserId FROM Librarians WHERE LibrarianId = @Id)
                OR UserId = @Id", con);
            updateUserCmd.Parameters.AddWithValue("@Id", model.LibrarianId);
            updateUserCmd.Parameters.AddWithValue("@Name", model.Name ?? "");
            updateUserCmd.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            updateUserCmd.ExecuteNonQuery();

            TempData["Success"] = "Librarian updated successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Get UserId first
            int userId = 0;
            using (var getCmd = new SqlCommand("SELECT UserId FROM Librarians WHERE LibrarianId = @Id", con))
            {
                getCmd.Parameters.AddWithValue("@Id", id);
                var result = getCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    userId = (int)result;
            }

            // Delete from Librarians
            var cmd = new SqlCommand("DELETE FROM Librarians WHERE LibrarianId = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            // Deactivate user
            if (userId > 0)
            {
                var deactivateCmd = new SqlCommand("UPDATE Users SET IsActive = 0 WHERE UserId = @UserId", con);
                deactivateCmd.Parameters.AddWithValue("@UserId", userId);
                deactivateCmd.ExecuteNonQuery();
            }
            else
            {
                // If UserId was 0, try using the id as UserId directly
                var deactivateCmd = new SqlCommand("UPDATE Users SET IsActive = 0 WHERE UserId = @UserId AND Role = 'Librarian'", con);
                deactivateCmd.Parameters.AddWithValue("@UserId", id);
                deactivateCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Librarian deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}