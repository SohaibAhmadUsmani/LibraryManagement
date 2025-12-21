using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class BookController : Controller
    {
        private readonly IConfiguration _config;

        public BookController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            var books = new List<BookModel>();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT BookId, BookName, Author, ISNULL(Publisher, '') as Publisher, ISNULL(Quantity, 1) as Quantity FROM Books ORDER BY BookName", con);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var book = new BookModel();
                        book.BookId = (int)reader["BookId"];
                        book.BookName = reader["BookName"].ToString();
                        book.Author = reader["Author"].ToString();
                        book.Publisher = reader["Publisher"].ToString();
                        book.Quantity = (int)reader["Quantity"];
                        books.Add(book);
                    }
                }
            }

            return View(books);
        }

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(BookModel model)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(model.BookName))
            {
                ViewBag.Error = "Book name is required.";
                return View(model);
            }

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO Books (BookName, Author, Publisher, Quantity) VALUES (@BookName, @Author, @Publisher, @Quantity)", con);
                cmd.Parameters.AddWithValue("@BookName", model.BookName);
                cmd.Parameters.AddWithValue("@Author", model.Author ?? "");
                cmd.Parameters.AddWithValue("@Publisher", model.Publisher ?? "");
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity > 0 ? model.Quantity : 1);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Book added successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new BookModel();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT BookId, BookName, Author, ISNULL(Publisher, '') as Publisher, ISNULL(Quantity, 1) as Quantity FROM Books WHERE BookId = @BookId", con);
                cmd.Parameters.AddWithValue("@BookId", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.BookId = (int)reader["BookId"];
                        model.BookName = reader["BookName"].ToString();
                        model.Author = reader["Author"].ToString();
                        model.Publisher = reader["Publisher"].ToString();
                        model.Quantity = (int)reader["Quantity"];
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(BookModel model)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(model.BookName))
            {
                ViewBag.Error = "Book name is required.";
                return View(model);
            }

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("UPDATE Books SET BookName = @BookName, Author = @Author, Publisher = @Publisher, Quantity = @Quantity WHERE BookId = @BookId", con);
                cmd.Parameters.AddWithValue("@BookId", model.BookId);
                cmd.Parameters.AddWithValue("@BookName", model.BookName);
                cmd.Parameters.AddWithValue("@Author", model.Author ?? "");
                cmd.Parameters.AddWithValue("@Publisher", model.Publisher ?? "");
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity > 0 ? model.Quantity : 1);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Book updated successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Librarian")
            {
                return RedirectToAction("Index", "Login");
            }

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE BookId = @BookId AND Status = 'Approved'", con);
                checkCmd.Parameters.AddWithValue("@BookId", id);
                int borrowed = (int)checkCmd.ExecuteScalar();

                if (borrowed > 0)
                {
                    TempData["Error"] = "Cannot delete book.  It is currently borrowed.";
                    return RedirectToAction("Index");
                }

                SqlCommand cmd = new SqlCommand("DELETE FROM Books WHERE BookId = @BookId", con);
                cmd.Parameters.AddWithValue("@BookId", id);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}