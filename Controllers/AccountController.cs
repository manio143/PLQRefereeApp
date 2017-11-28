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

        [Route("/account/pwdchange")]
        [HttpGet]
        [Authorize]
        public IActionResult PasswordChangeForm()
        {
            return View("PasswordChange");
        }

        [Route("/account/pwdchange")]
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult PasswordChangePost([FromForm]string oldpwd, [FromForm]string newpwd)
        {
            var auth = new Authentication(UserRepository);
            var user = HttpContext.Session.GetUser(UserRepository);
            
            if(!auth.TryAuthenticateUser(user.Email, oldpwd, out var usr))
                ViewBag.Error = true;
            else {
                var passphrase = BCrypt.Net.BCrypt.HashPassword(newpwd, 13);
                UserRepository.ChangePassword(user, passphrase);
                ViewBag.Success = true;
            }

            return View("PasswordChange");
        }

        [Route("/account/teamchange")]
        [HttpGet]
        [Authorize]
        public IActionResult TeamChangeForm()
        {
            return View("TeamChange");
        }

        [Route("/account/teamchange")]
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult TeamChangePost([FromForm]string team)
        {
            var user = HttpContext.Session.GetUser(UserRepository);
            UserRepository.ChangeTeam(user, team);
            return LocalRedirect("/account/details");
        }
    }
}