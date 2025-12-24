using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;
using LibraryManagement.Models.Entities;

namespace LibraryManagement.Controllers
{
    public class LibrarianDashboardController : Controller
    {
        private readonly IConfiguration _config;

        public LibrarianDashboardController(IConfiguration config)
        {
            _config = config;
        }

        private bool IsLibrarian()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Librarian";
        }

        public IActionResult Index()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new LibrarianDashboardModel
            {
                LibrarianName = HttpContext.Session.GetString("FullName") ?? "Librarian",
                RecentBorrowings = new List<BorrowingModel>(),
                TotalStudents = 0,
                TotalBooks = 0,
                TotalBorrowings = 0,
                OverdueBorrowings = 0
            };

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                // Total Students
                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Student' AND IsActive = 1", con);
                model.TotalStudents = (int)cmd1.ExecuteScalar();

                // Total Books
                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM Books", con);
                model.TotalBooks = (int)cmd2.ExecuteScalar();

                // Active Borrowings
                SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE Status = 'Approved'", con);
                model.TotalBorrowings = (int)cmd3.ExecuteScalar();

                // Overdue Borrowings
                SqlCommand cmd4 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE Status = 'Approved' AND DueDate < GETDATE()", con);
                model.OverdueBorrowings = (int)cmd4.ExecuteScalar();

                // Pending Requests Count
                SqlCommand cmd5 = new SqlCommand("SELECT COUNT(*) FROM BorrowRequests WHERE Status = 'Pending'", con);
                ViewBag.PendingRequests = (int)cmd5.ExecuteScalar();
            }

            return View(model);
        }

        public IActionResult PendingRequests()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Index", "Login");
            }

            var requests = new List<BorrowRequestModel>();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT RequestId, UserId, StudentName, BookId, BookName, RequestDate, Status FROM BorrowRequests WHERE Status = 'Pending' ORDER BY RequestDate", con);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var req = new BorrowRequestModel();
                        req.RequestId = (int)reader["RequestId"];
                        req.UserId = (int)reader["UserId"];
                        req.StudentName = reader["StudentName"].ToString();
                        req.BookName = reader["BookName"].ToString();
                        req.BookId = (int)reader["BookId"];
                        req.RequestDate = (DateTime)reader["RequestDate"];
                        req.Status = reader["Status"].ToString();
                        requests.Add(req);
                    }
                }
            }

            return View(requests);
        }

        [HttpPost]
        public IActionResult ApproveRequest(int requestId)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Index", "Login");
            }

            var librarianId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var dueDate = DateTime.Now.AddDays(14);
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                // Get BookId from the request
                int bookId = 0;
                SqlCommand getCmd = new SqlCommand("SELECT BookId FROM BorrowRequests WHERE RequestId = @RequestId", con);
                getCmd.Parameters.AddWithValue("@RequestId", requestId);
                var result = getCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    bookId = (int)result;
                }

                if (bookId == 0)
                {
                    TempData["Error"] = "Request not found.  RequestId: " + requestId;
                    return RedirectToAction("PendingRequests");
                }

                // Check book quantity
                int quantity = 0;
                SqlCommand qtyCmd = new SqlCommand("SELECT ISNULL(Quantity, 5) FROM Books WHERE BookId = @BookId", con);
                qtyCmd.Parameters.AddWithValue("@BookId", bookId);
                var qtyResult = qtyCmd.ExecuteScalar();
                if (qtyResult != null && qtyResult != DBNull.Value)
                {
                    quantity = (int)qtyResult;
                }

                if (quantity <= 0)
                {
                    TempData["Error"] = "Book is not available. No copies left. ";
                    return RedirectToAction("PendingRequests");
                }

                // Approve the request
                SqlCommand approveCmd = new SqlCommand("UPDATE BorrowRequests SET Status = 'Approved', ApprovedDate = GETDATE(), ApprovedBy = @LibrarianId, DueDate = @DueDate WHERE RequestId = @RequestId", con);
                approveCmd.Parameters.AddWithValue("@RequestId", requestId);
                approveCmd.Parameters.AddWithValue("@LibrarianId", librarianId);
                approveCmd.Parameters.AddWithValue("@DueDate", dueDate);
                int rowsAffected = approveCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    TempData["Error"] = "Failed to approve request. ";
                    return RedirectToAction("PendingRequests");
                }

                // Decrease book quantity
                SqlCommand updateCmd = new SqlCommand("UPDATE Books SET Quantity = Quantity - 1 WHERE BookId = @BookId", con);
                updateCmd.Parameters.AddWithValue("@BookId", bookId);
                updateCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Request approved!  Due date: " + dueDate.ToString("MMM dd, yyyy");
            return RedirectToAction("PendingRequests");
        }

        [HttpPost]
        public IActionResult RejectRequest(int requestId)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Index", "Login");
            }

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("UPDATE BorrowRequests SET Status = 'Rejected' WHERE RequestId = @RequestId", con);
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    TempData["Success"] = "Request rejected. ";
                }
                else
                {
                    TempData["Error"] = "Failed to reject request.";
                }
            }

            return RedirectToAction("PendingRequests");
        }

        public IActionResult ActiveBorrowings()
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Index", "Login");
            }

            var borrowings = new List<BorrowRequestModel>();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT RequestId, UserId, StudentName, BookId, BookName, ApprovedDate, DueDate, Status FROM BorrowRequests WHERE Status = 'Approved' ORDER BY DueDate", con);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var req = new BorrowRequestModel();
                        req.RequestId = (int)reader["RequestId"];
                        req.StudentName = reader["StudentName"].ToString();
                        req.BookName = reader["BookName"].ToString();
                        req.BookId = (int)reader["BookId"];

                        if (reader["ApprovedDate"] != DBNull.Value)
                        {
                            req.ApprovedDate = (DateTime)reader["ApprovedDate"];
                        }

                        if (reader["DueDate"] != DBNull.Value)
                        {
                            req.DueDate = (DateTime)reader["DueDate"];
                        }

                        borrowings.Add(req);
                    }
                }
            }

            return View(borrowings);
        }

        [HttpPost]
        public IActionResult ReturnBook(int requestId)
        {
            if (!IsLibrarian())
            {
                return RedirectToAction("Index", "Login");
            }

            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                // Get book ID
                int bookId = 0;
                SqlCommand getCmd = new SqlCommand("SELECT BookId FROM BorrowRequests WHERE RequestId = @RequestId", con);
                getCmd.Parameters.AddWithValue("@RequestId", requestId);
                var result = getCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    bookId = (int)result;
                }

                // Mark as returned
                SqlCommand returnCmd = new SqlCommand("UPDATE BorrowRequests SET Status = 'Returned', ReturnedDate = GETDATE() WHERE RequestId = @RequestId", con);
                returnCmd.Parameters.AddWithValue("@RequestId", requestId);
                returnCmd.ExecuteNonQuery();

                // Increase book quantity
                if (bookId > 0)
                {
                    SqlCommand updateCmd = new SqlCommand("UPDATE Books SET Quantity = Quantity + 1 WHERE BookId = @BookId", con);
                    updateCmd.Parameters.AddWithValue("@BookId", bookId);
                    updateCmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Book returned successfully!";
            return RedirectToAction("ActiveBorrowings");
        }
    }
}