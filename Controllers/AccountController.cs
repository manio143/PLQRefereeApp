using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class AccountController : Controller
    {
        private UserRepository UserRepository { get; }
        private TestRepository TestRepository { get; }
        public AccountController(UserRepository userRepository, TestRepository testRepository)
        {
            UserRepository = userRepository;
            TestRepository = testRepository;
        }

        [Route("/profile/{id}")]
        [HttpGet]
        public IActionResult Profile(int id)
        {
            var data = UserRepository.GetUserData(id);
            ViewBag.Options = false;            
            return View("Account", data);
        }

        [Route("/account/details")]
        [HttpGet]
        [Authorize]
        public IActionResult Account()
        {
            var user = HttpContext.Session.GetUser(UserRepository);
            var data = UserRepository.GetUserData(user);
            var tests = TestRepository.GetTestsFor(user).Where(t => t.Finished != null);
            ViewBag.Tests = tests;
            ViewBag.Options = true;
            return View("Account", data);
        }
    }
}