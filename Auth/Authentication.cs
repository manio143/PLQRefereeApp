using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace PLQRefereeApp
{
    public class Authentication
    {
        public static string Scheme => "Cookies"; //same as default for CookieAuthentication

        private UserRepository UserRepository { get; }
        public Authentication(UserRepository userRepository)
        {
            UserRepository = userRepository;
        }

        public bool TryAuthenticateUser(string email, string password, out User outUser)
        {
            outUser = null;

            if (!UserRepository.UserExists(email))
                return false;

            var user = UserRepository.GetUser(email);

            if (!BCrypt.Net.BCrypt.Verify(password, user.Passphrase))
                return false;

            outUser = user;
            return true;
        }

        public static async Task ValidatePrincipal(CookieValidatePrincipalContext arg)
        {
            var session = arg.HttpContext.Session;
            if (session.GetInt32("UserId") == null)
            {
                arg.RejectPrincipal();
                await arg.HttpContext.SignOutAsync(Scheme);
            }
        }
    }
}