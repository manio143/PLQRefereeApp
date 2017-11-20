using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public static class SessionExtension
    {
        public static User GetUser(this ISession session, UserRepository userRepository) {
            return userRepository.GetUser(session.GetInt32("UserId").Value);
        }
    }
}