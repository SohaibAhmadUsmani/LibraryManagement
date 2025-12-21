using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class BorrowingController : Controller
    {
        private readonly IConfiguration _config;

        public BorrowingController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var borrowings = new List<BorrowRequestModel>();
            string connString = _config.GetConnectionString("DefaultConnection");

            using (var con = new SqlConnection(connString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT RequestId, StudentName, BookName, RequestDate, Status, ApprovedDate, DueDate, ReturnedDate FROM BorrowRequests ORDER BY RequestDate DESC", con);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var req = new BorrowRequestModel();
                        req.RequestId = (int)reader["RequestId"];
                        req.StudentName = reader["StudentName"].ToString();
                        req.BookName = reader["BookName"].ToString();
                        req.RequestDate = (DateTime)reader["RequestDate"];
                        req.Status = reader["Status"].ToString();

                        if (reader["ApprovedDate"] != DBNull.Value)
                        {
                            req.ApprovedDate = (DateTime)reader["ApprovedDate"];
                        }

                        if (reader["DueDate"] != DBNull.Value)
                        {
                            req.DueDate = (DateTime)reader["DueDate"];
                        }

                        if (reader["ReturnedDate"] != DBNull.Value)
                        {
                            req.ReturnedDate = (DateTime)reader["ReturnedDate"];
                        }

                        borrowings.Add(req);
                    }
                }
            }

            return View(borrowings);
        }
    }
}