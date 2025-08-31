using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public List<LoginModel> PutValue()
        {
            var users = new List<LoginModel>
            {
                new LoginModel{id=1,username="admin",password="123"},
                new LoginModel{id=2,username="mycodingproject",password="myc546"},

            };

            return users;
        }

        [HttpPost]
        public IActionResult Verify(LoginModel usr)
        {
            var u = PutValue();

            var ue = u.Where(u => u.username.Equals(usr.username));
            var up = ue.Where(p => p.password.Equals(usr.password));

            if (up.Count() == 1)
            {
                TempData["message"] = "Login Success";
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.message = "Login Failed";
                return View("Index");
            }
        }
    }
}
