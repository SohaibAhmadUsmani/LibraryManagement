using Microsoft.AspNetCore.Mvc;
using System.Data;
using LibraryManagement.Models;
using Microsoft.Data.SqlClient;

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
            List<BorrowingModel> list = new();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("SELECT * FROM Borrowings", con);
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new BorrowingModel
                {
                    BorrowingId = Convert.ToInt32(reader["BorrowingId"]),
                    StudentName = reader["StudentName"].ToString(),
                    BookName = reader["BookName"].ToString(),
                    BorrowDate = Convert.ToDateTime(reader["BorrowDate"]),
                    ReturnedDate = Convert.ToDateTime(reader["ReturnedDate"])
                });
            }
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(BorrowingModel model)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("INSERT INTO Borrowings (StudentName, BookName, BorrowDate, ReturnedDate) VALUES (@studentname, @bookname, @borrowdate, @returneddate)", con);
            cmd.Parameters.AddWithValue("@studentname", model.StudentName);
            cmd.Parameters.AddWithValue("@bookname", model.BookName);
            cmd.Parameters.AddWithValue("@borrowdate", model.BorrowDate);
            cmd.Parameters.AddWithValue("@returneddate", model.ReturnedDate);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            BorrowingModel model = new();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("SELECT * FROM Borrowings WHERE BorrowingId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.BorrowingId = Convert.ToInt32(reader["BorrowingId"]);
                model.StudentName = reader["StudentName"].ToString();
                model.BookName = reader["BookName"].ToString();
                model.BorrowDate = Convert.ToDateTime(reader["BorrowDate"]);
                model.ReturnedDate = Convert.ToDateTime(reader["ReturnedDate"]);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(BorrowingModel model)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("UPDATE Borrowings SET StudentName=@studentname, BookName=@bookname, BorrowDate=@borrowdate, ReturnedDate=@returneddate WHERE BorrowingId=@id", con);
            cmd.Parameters.AddWithValue("@studentname", model.StudentName);
            cmd.Parameters.AddWithValue("@bookname", model.BookName);
            cmd.Parameters.AddWithValue("@borrowdate", model.BorrowDate);
            cmd.Parameters.AddWithValue("@returneddate", model.ReturnedDate);
            cmd.Parameters.AddWithValue("@id", model.BorrowingId);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("DELETE FROM Borrowings WHERE BorrowingId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
