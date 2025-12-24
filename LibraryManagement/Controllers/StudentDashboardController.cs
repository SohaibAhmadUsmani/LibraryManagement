using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;
using LibraryManagement.Models.Entities;

namespace LibraryManagement.Controllers
{
    public class StudentDashboardController : Controller
    {
        private readonly IConfiguration _config;

        public StudentDashboardController(IConfiguration config)
        {
            _config = config;
        }

        private bool IsStudent()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Student";
        }

        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        private string GetStudentName()
        {
            return HttpContext.Session.GetString("FullName") ?? "";
        }

        public IActionResult Index()
        {
            if (!IsStudent())
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = GetUserId();
            var studentName = GetStudentName();

            var model = new StudentDashboardModel
            {
                StudentName = studentName,
                MyBorrowedBooks = 0,
                OverdueBooks = 0,
                AvailableBooks = 0,
                PendingRequests = 0,
                RecentBooks = new List<BookModel>(),
                MyRequests = new List<BorrowRequestModel>()
            };

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE UserId = @UserId AND Status = 'Approved'", con);
                cmd1.Parameters.AddWithValue("@UserId", userId);
                model.MyBorrowedBooks = (int)cmd1.ExecuteScalar();

                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE UserId = @UserId AND Status = 'Approved' AND DueDate < GETDATE()", con);
                cmd2.Parameters.AddWithValue("@UserId", userId);
                model.OverdueBooks = (int)cmd2.ExecuteScalar();

                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM Books", con);
                model.AvailableBooks = (int)cmd3.ExecuteScalar();

                SqlCommand cmd4 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE UserId = @UserId AND Status = 'Pending'", con);
                cmd4.Parameters.AddWithValue("@UserId", userId);
                model.PendingRequests = (int)cmd4.ExecuteScalar();

                SqlCommand cmd5 = new SqlCommand("SELECT TOP 5 BookId, BookName, Author, ISNULL(Quantity, 1) as Quantity FROM Books ORDER BY BookId DESC", con);
                using (var reader5 = cmd5.ExecuteReader())
                {
                    while (reader5.Read())
                    {
                        model.RecentBooks.Add(new BookModel
                        {
                            BookId = (int)reader5["BookId"],
                            BookName = reader5["BookName"].ToString(),
                            Author = reader5["Author"].ToString(),
                            Quantity = (int)reader5["Quantity"]
                        });
                    }
                }

                SqlCommand cmd6 = new SqlCommand("SELECT RequestId, BookName, RequestDate, Status, DueDate FROM BorrowRequests WHERE UserId = @UserId AND Status IN ('Approved', 'Pending') ORDER BY RequestDate DESC", con);
                cmd6.Parameters.AddWithValue("@UserId", userId);
                using (var reader6 = cmd6.ExecuteReader())
                {
                    while (reader6.Read())
                    {
                        var req = new BorrowRequestModel();
                        req.RequestId = (int)reader6["RequestId"];
                        req.BookName = reader6["BookName"].ToString();
                        req.RequestDate = (DateTime)reader6["RequestDate"];
                        req.Status = reader6["Status"].ToString();
                        if (reader6["DueDate"] != DBNull.Value)
                        {
                            req.DueDate = (DateTime)reader6["DueDate"];
                        }
                        model.MyRequests.Add(req);
                    }
                }
            }

            return View(model);
        }

        public IActionResult SearchBooks(string searchTerm)
        {
            if (!IsStudent())
            {
                return RedirectToAction("Index", "Login");
            }

            var books = new List<BookModel>();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                string query = "SELECT BookId, BookName, Author, ISNULL(Quantity, 1) as Quantity FROM Books WHERE ISNULL(Quantity, 1) > 0";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " AND (BookName LIKE @Search OR Author LIKE @Search)";
                }
                query += " ORDER BY BookName";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new BookModel
                        {
                            BookId = (int)reader["BookId"],
                            BookName = reader["BookName"].ToString(),
                            Author = reader["Author"].ToString(),
                            Quantity = (int)reader["Quantity"]
                        });
                    }
                }
            }

            ViewBag.SearchTerm = searchTerm;
            return View(books);
        }

        [HttpPost]
        public IActionResult RequestBook(int bookId)
        {
            if (!IsStudent())
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = GetUserId();
            var studentName = GetStudentName();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE UserId = @UserId AND BookId = @BookId AND Status IN ('Pending', 'Approved')", con);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                checkCmd.Parameters.AddWithValue("@BookId", bookId);
                int existing = (int)checkCmd.ExecuteScalar();

                if (existing > 0)
                {
                    TempData["Error"] = "You already have a pending or active request for this book.";
                    return RedirectToAction("SearchBooks");
                }

                string bookName = "";
                SqlCommand getBookCmd = new SqlCommand("SELECT BookName FROM Books WHERE BookId = @BookId", con);
                getBookCmd.Parameters.AddWithValue("@BookId", bookId);
                var result = getBookCmd.ExecuteScalar();
                if (result != null)
                {
                    bookName = result.ToString();
                }

                SqlCommand insertCmd = new SqlCommand("INSERT INTO BorrowRequests (UserId, StudentName, BookId, BookName, RequestDate, Status) VALUES (@UserId, @StudentName, @BookId, @BookName, GETDATE(), 'Pending')", con);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                insertCmd.Parameters.AddWithValue("@StudentName", studentName);
                insertCmd.Parameters.AddWithValue("@BookId", bookId);
                insertCmd.Parameters.AddWithValue("@BookName", bookName);
                insertCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Book request submitted!  Please wait for librarian approval.";
            return RedirectToAction("MyBorrowings");
        }

        public IActionResult MyBorrowings()
        {
            if (!IsStudent())
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = GetUserId();
            var requests = new List<BorrowRequestModel>();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT RequestId, BookId, BookName, RequestDate, Status, DueDate, ReturnedDate FROM BorrowRequests WHERE UserId = @UserId ORDER BY RequestDate DESC", con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var req = new BorrowRequestModel();
                        req.RequestId = (int)reader["RequestId"];
                        req.BookId = (int)reader["BookId"];
                        req.BookName = reader["BookName"].ToString();
                        req.RequestDate = (DateTime)reader["RequestDate"];
                        req.Status = reader["Status"].ToString();

                        if (reader["DueDate"] != DBNull.Value)
                        {
                            req.DueDate = (DateTime)reader["DueDate"];
                        }

                        if (reader["ReturnedDate"] != DBNull.Value)
                        {
                            req.ReturnedDate = (DateTime)reader["ReturnedDate"];
                        }

                        requests.Add(req);
                    }
                }
            }

            return View(requests);
        }

        [HttpPost]
        public IActionResult ReturnBook(IFormCollection form)
        {
            if (!IsStudent())
            {
                return RedirectToAction("Index", "Login");
            }

            string reqIdStr = form["requestId"].ToString();
            int requestId = 0;
            int.TryParse(reqIdStr, out requestId);

            if (requestId == 0)
            {
                TempData["Error"] = "Invalid request ID.";
                return RedirectToAction("MyBorrowings");
            }

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                int bookId = 0;
                string status = "";

                SqlCommand checkCmd = new SqlCommand("SELECT BookId, Status FROM BorrowRequests WHERE RequestId = @RequestId", con);
                checkCmd.Parameters.AddWithValue("@RequestId", requestId);

                using (var reader = checkCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bookId = (int)reader["BookId"];
                        status = reader["Status"].ToString();
                    }
                }

                if (bookId == 0)
                {
                    TempData["Error"] = "Request not found.";
                    return RedirectToAction("MyBorrowings");
                }

                if (status != "Approved")
                {
                    TempData["Error"] = "Book cannot be returned.  Status: " + status;
                    return RedirectToAction("MyBorrowings");
                }

                SqlCommand returnCmd = new SqlCommand("UPDATE BorrowRequests SET Status = 'Returned', ReturnedDate = GETDATE() WHERE RequestId = @RequestId", con);
                returnCmd.Parameters.AddWithValue("@RequestId", requestId);
                returnCmd.ExecuteNonQuery();

                SqlCommand updateCmd = new SqlCommand("UPDATE Books SET Quantity = Quantity + 1 WHERE BookId = @BookId", con);
                updateCmd.Parameters.AddWithValue("@BookId", bookId);
                updateCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Book returned successfully!";
            return RedirectToAction("MyBorrowings");
        }

        public IActionResult Profile()
        {
            if (!IsStudent())
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = GetUserId();
            var user = new UserModel();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT UserId, Username, Email, FullName, Phone, Role FROM Users WHERE UserId = @UserId", con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user.UserId = (int)reader["UserId"];
                        user.Username = reader["Username"].ToString();
                        user.Email = reader["Email"].ToString();
                        user.FullName = reader["FullName"].ToString();
                        user.Phone = reader["Phone"].ToString();
                        user.Role = reader["Role"].ToString();
                    }
                }
            }

            return View(user);
        }
    }
}