using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class AdminController : Controller
    {
        private UserRepository UserRepository { get; }
        private TestRepository TestRepository { get; }
        public AdminController(UserRepository userRepository, TestRepository testRepository)
        {
            UserRepository = userRepository;
            TestRepository = testRepository;
        }

        private bool Allow() {
            return HttpContext.Session.GetUser(UserRepository).Administrator;
        }

        [HttpGet("/admin")]
        [Authorize]
        public IActionResult Panel(string error) {
            if(!Allow())
                return LocalRedirect("/");
            ViewBag.Error = error;
            return View("Admin");
        }
    }
}