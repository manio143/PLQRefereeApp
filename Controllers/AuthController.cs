using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace PLQRefereeApp
{
    public class LoginPostData
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterPostData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Team { get; set; }
    }

    public enum RegisterError
    {
        EmailAlreadyInUse,
        InvalidValue
    }

    public class AuthControler : Controller
    {
        private UserRepository UserRepository { get; }
        private Authentication Authentication { get; }
        public AuthControler(UserRepository userRepository)
        {
            UserRepository = userRepository;
            Authentication = new Authentication(userRepository);
        }

        [Route("/login")]
        [HttpGet]
        public IActionResult LoginView(string returnUrl = null)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                return LocalRedirect(returnUrl ?? "/");
            return View("Login");
        }

        [Route("/login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPost(LoginPostData loginData, string returnUrl = null)
        {
            if (Authentication.TryAuthenticateUser(loginData.Email, loginData.Password, out var user))
            {
                await SetAuthSession(user);
                return LocalRedirect(returnUrl ?? "/");
            }
            else
            {
                ViewBag.Error = true;
                return View("Login");
            }
        }

        [Route("/logout")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Authentication.Scheme);
            return LocalRedirect("/");
        }

        private ClaimsPrincipal ConstructClaimsPrincipal(User user)
        {
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Administrator ? "Admin" : "User")
            }, Authentication.Scheme)
            {
                Label = user.Email
            };

            return new ClaimsPrincipal(identity);
        }

        private async Task SetAuthSession(User user)
        {
            await HttpContext.SignInAsync(Authentication.Scheme, ConstructClaimsPrincipal(user));
            HttpContext.Session.SetInt32("UserId", user.Id);
        }

        [Route("/register")]
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                return LocalRedirect("/");
            return View("Register");
        }

        [Route("/register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPost(RegisterPostData registerData)
        {
            if (UserRepository.UserExists(registerData.Email))
            {
                ViewBag.Error = RegisterError.EmailAlreadyInUse;
                return View("Register");
            }

            if (String.IsNullOrWhiteSpace(registerData.Name)
                || String.IsNullOrWhiteSpace(registerData.Surname)
                || String.IsNullOrWhiteSpace(registerData.Email)
                || String.IsNullOrWhiteSpace(registerData.Password))
            {
                ViewBag.Error = RegisterError.InvalidValue;
                return View("Register");
            }

            var passphrase = BCrypt.Net.BCrypt.HashPassword(registerData.Password, 13); //2^13 iterations

            var user = new User { Email = registerData.Email, Passphrase = passphrase };
            var userData = new UserData { Name = registerData.Name, Surname = registerData.Surname, Team = registerData.Surname };

            UserRepository.AddUser(user, userData);

            await SetAuthSession(user);
            return LocalRedirect("/");
        }
    }
}