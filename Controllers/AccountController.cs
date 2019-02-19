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

        [HttpGet("/profile/{id}")]
        public IActionResult Profile(int id)
        {
            var user = UserRepository.GetUser(id);
            var data = UserRepository.GetUserData(id);
            var arCerts = TestRepository.GetTestsFor(user).Where(t => (t.Mark >= 80 || t.IQA) && t.Type == "AR")
                            .Select(t => t.Rulebook).Distinct().OrderByDescending(x => x).ToArray();
            var srCerts = TestRepository.GetTestsFor(user).Where(t => (t.Mark >= 80 || t.IQA) && t.Type == "SR")
                            .Select(t => t.Rulebook).Distinct().OrderByDescending(x => x).ToArray();
            var hrCerts = TestRepository.GetTestsFor(user).Where(t => (t.Mark >= 80 || t.IQA) && t.Type == "HR")
                            .Select(t => t.Rulebook).Distinct().OrderByDescending(x => x).ToArray();
            ViewBag.ARCerts = arCerts.String();
            ViewBag.SRCerts = srCerts.String();
            ViewBag.HRCerts = hrCerts.String();
            ViewBag.Options = false;
            return View("Account", data);
        }

        [Authorize]
        [HttpGet("/account/details")]
        public IActionResult Account()
        {
            var user = HttpContext.Session.GetUser(UserRepository);
            var data = UserRepository.GetUserData(user);
            var tests = TestRepository.GetTestsFor(user).Where(t => t.Finished != null);
            var arCerts = TestRepository.GetTestsFor(user).Where(t => (t.Mark >= 80 || t.IQA) && t.Type == "AR")
                            .Select(t => t.Rulebook).Distinct().OrderByDescending(x => x).ToArray();
            var srCerts = TestRepository.GetTestsFor(user).Where(t => (t.Mark >= 80 || t.IQA) && t.Type == "SR")
                            .Select(t => t.Rulebook).Distinct().OrderByDescending(x => x).ToArray();
            var hrCerts = TestRepository.GetTestsFor(user).Where(t => (t.Mark >= 80 || t.IQA) && t.Type == "HR")
                            .Select(t => t.Rulebook).Distinct().OrderByDescending(x => x).ToArray();
            ViewBag.ARCerts = arCerts.String();
            ViewBag.SRCerts = srCerts.String();
            ViewBag.HRCerts = hrCerts.String();
            ViewBag.Tests = tests;
            ViewBag.Options = true;
            return View("Account", data);
        }

        [Authorize]
        [HttpGet("/account/pwdchange")]
        public IActionResult PasswordChangeForm()
        {
            return View("PasswordChange");
        }

        [Authorize]
        [HttpPost("/account/pwdchange")]
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

        [Authorize]
        [HttpGet("/account/teamchange")]
        public IActionResult TeamChangeForm()
        {
            return View("TeamChange");
        }

        [Authorize]
        [HttpPost("/account/teamchange")]
        [ValidateAntiForgeryToken]
        public IActionResult TeamChangePost([FromForm]string team)
        {
            var user = HttpContext.Session.GetUser(UserRepository);
            UserRepository.ChangeTeam(user, team.Trim());
            return LocalRedirect("/account/details");
        }
    }
}